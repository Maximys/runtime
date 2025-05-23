// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Text.Encodings.Web
{
    /// <summary>
    /// Allows efficient escaping for the library's built-in types (HTML, URL, JS).
    /// Assumes the following:
    ///   (a) All C0 and C1 code points are disallowed.
    ///   (b) Escaping 1 ASCII input character results in no more than 6 output characters.
    ///   (c) All Unicode scalar values may be represented in escaped form.
    ///   (d) The escaped form of any Unicode scalar value consists of only ASCII characters.
    /// </summary>
    internal sealed partial class OptimizedInboxTextEncoder
    {
        private readonly AsciiPreescapedData _asciiPreescapedData;
        private readonly AllowedBmpCodePointsBitmap _allowedBmpCodePoints;
        private readonly ScalarEscaperBase _scalarEscaper;

#if NET
        private readonly SearchValues<byte> _allowedAsciiBytes;
        private readonly SearchValues<char> _allowedAsciiChars;
#endif

        internal OptimizedInboxTextEncoder(
            ScalarEscaperBase scalarEscaper,
            in AllowedBmpCodePointsBitmap allowedCodePointsBmp,
            bool forbidHtmlSensitiveCharacters = true,
            ReadOnlySpan<char> extraCharactersToEscape = default)
        {
            Debug.Assert(scalarEscaper != null);

            _scalarEscaper = scalarEscaper;
            _allowedBmpCodePoints = allowedCodePointsBmp;

#if DEBUG
            // Debug-only assertion to validate that we're no longer using the input
            // argument once the field value has been assigned. All accesses to the bitmap
            // should now go through our instance field. In debug mode, if any code violates
            // this, it'll cause a null ref within this ctor.
            allowedCodePointsBmp = ref Unsafe.NullRef<AllowedBmpCodePointsBitmap>();
#endif

            // Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
            // (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp).
            _allowedBmpCodePoints.ForbidUndefinedCharacters();

            // Most encoders should forbid characters that are special in HTML, even if they're not
            // HTML encoders themselves. This is defense-in-depth for scenarios where somebody encodes
            // a JavaScript string or a URL, then places it straight into an HTML document without
            // accounting for any required outer envelope (HTML) escaping.
            if (forbidHtmlSensitiveCharacters)
            {
                _allowedBmpCodePoints.ForbidHtmlCharacters();
            }

            foreach (char ch in extraCharactersToEscape)
            {
                _allowedBmpCodePoints.ForbidChar(ch);
            }

            // Now that disallowed characters have been filtered out, we're free to populate
            // the ASCII maps and pre-escaped data caches.
            _asciiPreescapedData.PopulatePreescapedData(_allowedBmpCodePoints, scalarEscaper);

#if NET
            // Create the SearchValues instances for vectorized fast paths.
            Span<byte> allowedAsciiBytes = stackalloc byte[128];
            Span<char> allowedAsciiChars = stackalloc char[128];
            int allowedAsciiCount = 0;

            for (int i = 0; i < 128; i++)
            {
                if (_allowedBmpCodePoints.IsCharAllowed((char)i))
                {
                    allowedAsciiBytes[allowedAsciiCount] = (byte)i;
                    allowedAsciiChars[allowedAsciiCount] = (char)i;
                    allowedAsciiCount++;
                }
            }

            _allowedAsciiBytes = SearchValues.Create(allowedAsciiBytes.Slice(0, allowedAsciiCount));
            _allowedAsciiChars = SearchValues.Create(allowedAsciiChars.Slice(0, allowedAsciiCount));
#endif
        }

        [Obsolete("FindFirstCharacterToEncode has been deprecated. It should only be used by the TextEncoder adapter.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            return GetIndexOfFirstCharToEncode(new ReadOnlySpan<char>(text, textLength)); // performs bounds checking
        }

        [Obsolete("TryEncodeUnicodeScalar has been deprecated. It should only be used by the TextEncoder adapter.")]
        public unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            Span<char> destination = new Span<char>(buffer, bufferLength);

            if (_allowedBmpCodePoints.IsCodePointAllowed((uint)unicodeScalar))
            {
                // The bitmap should only allow BMP non-surrogate code points.
                UnicodeDebug.AssertIsBmpCodePoint((uint)unicodeScalar);
                UnicodeDebug.AssertIsValidScalar((uint)unicodeScalar);
                if (!destination.IsEmpty)
                {
                    destination[0] = (char)unicodeScalar; // reflect as-is
                    numberOfCharactersWritten = 1;
                    return true;
                }
            }
            else
            {
                int innerCharsWritten = _scalarEscaper.EncodeUtf16(new Rune(unicodeScalar), destination);
                Debug.Assert(innerCharsWritten <= bufferLength, "Mustn't overflow the buffer.");
                Debug.Assert(innerCharsWritten != 0, "Inner escaper succeeded with 0-char output?");
                if (innerCharsWritten >= 0)
                {
                    numberOfCharactersWritten = innerCharsWritten;
                    return true;
                }
            }

            // If we reached this point, we ran out of space in the destination.
            numberOfCharactersWritten = 0;
            return false;
        }

        public OperationStatus Encode(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
        {
            _AssertThisNotNull(); // hoist "this != null" check out of hot loop below

            int srcIdx = 0;
            int dstIdx = 0;

            while (true)
            {
                if ((uint)source.Length <= (uint)srcIdx)
                {
                    break; // EOF
                }

                char thisChar = source[srcIdx];
                if (!_asciiPreescapedData.TryGetPreescapedData(thisChar, out ulong preescapedEntry))
                {
                    goto NotAscii; // forward jump predicted not taken
                }

                if ((uint)destination.Length <= (uint)dstIdx)
                {
                    goto DestTooSmall; // forward jump predicted not taken
                }

                destination[dstIdx] = (char)(byte)preescapedEntry;
                if (((uint)preescapedEntry & 0xFF00) == 0)
                {
                    dstIdx++; // predicted taken - only had to write a single char
                    srcIdx++;
                    continue;
                }

                // At this point, we're writing a multi-char output for a single-char input.
                // Copy over as many chars as we can.

                preescapedEntry >>= 8;
                int dstIdxTemp = dstIdx + 1;
                do
                {
                    if ((uint)destination.Length <= (uint)dstIdxTemp)
                    {
                        goto DestTooSmall; // forward jump predicted not taken
                    }

                    destination[dstIdxTemp++] = (char)(byte)preescapedEntry;
                } while ((byte)(preescapedEntry >>= 8) != 0);

                dstIdx = dstIdxTemp;
                srcIdx++;
                continue;

            NotAscii:

                if (!Rune.TryCreate(thisChar, out Rune scalarValue))
                {
                    int srcIdxTemp = srcIdx + 1;
                    if ((uint)source.Length > (uint)srcIdxTemp)
                    {
                        if (Rune.TryCreate(thisChar, source[srcIdxTemp], out scalarValue))
                        {
                            goto CheckWhetherScalarValueAllowed; // successfully extracted scalar value
                        }
                    }
                    else if (!isFinalBlock && char.IsHighSurrogate(thisChar))
                    {
                        goto NeedMoreData; // ended with a high surrogate, and caller said they'd provide more data
                    }

                    scalarValue = Rune.ReplacementChar; // fallback char
                    goto MustEncodeNonAscii;
                }

            CheckWhetherScalarValueAllowed:

                if (IsScalarValueAllowed(scalarValue))
                {
                    if (!scalarValue.TryEncodeToUtf16(destination.Slice(dstIdx), out int utf16CodeUnitCount))
                    {
                        goto DestTooSmall;
                    }

                    dstIdx += utf16CodeUnitCount;
                    srcIdx += utf16CodeUnitCount;
                    continue;
                }

            MustEncodeNonAscii:

                // At this point, we know we need to encode.

                int charsWrittenJustNow = _scalarEscaper.EncodeUtf16(scalarValue, destination.Slice(dstIdx));
                if (charsWrittenJustNow < 0)
                {
                    goto DestTooSmall;
                }

                dstIdx += charsWrittenJustNow;
                srcIdx += scalarValue.Utf16SequenceLength;
            }

            // And at this point, we're done!

            OperationStatus retVal = OperationStatus.Done;

        CommonReturn:
            charsConsumed = srcIdx;
            charsWritten = dstIdx;
            return retVal;

        DestTooSmall:
            retVal = OperationStatus.DestinationTooSmall;
            goto CommonReturn;

        NeedMoreData:
            retVal = OperationStatus.NeedMoreData;
            goto CommonReturn;
        }

        public OperationStatus EncodeUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
        {
            _AssertThisNotNull(); // hoist "this != null" check out of hot loop below

            int srcIdx = 0;
            int dstIdx = 0;

            while (true)
            {
                if ((uint)source.Length <= (uint)srcIdx)
                {
                    break; // EOF
                }

                uint thisByte = source[srcIdx];
                if (!_asciiPreescapedData.TryGetPreescapedData(thisByte, out ulong preescapedEntry))
                {
                    goto NotAscii; // forward jump predicted not taken
                }

                // The common case is that the destination is large enough to hold 8 bytes of output,
                // so let's write the entire pre-escaped entry to it. In reality we're only writing up
                // to 6 bytes of output, so we'll only bump dstIdx by the number of useful bytes we
                // wrote.

                if (BinaryPrimitives.TryWriteUInt64LittleEndian(destination.Slice(dstIdx), preescapedEntry))
                {
                    dstIdx += (int)(preescapedEntry >> 56); // predicted taken
                    srcIdx++;
                    continue;
                }

                // We don't have enough space to hold a single QWORD copy, so let's write byte-by-byte
                // and see if we have enough room.

                int dstIdxTemp = dstIdx;
                do
                {
                    if ((uint)destination.Length <= (uint)dstIdxTemp)
                    {
                        goto DestTooSmall; // forward jump predicted not taken
                    }

                    destination[dstIdxTemp++] = (byte)preescapedEntry;
                } while ((byte)(preescapedEntry >>= 8) != 0);

                dstIdx = dstIdxTemp;
                srcIdx++;
                continue;

            NotAscii:

                OperationStatus runeDecodeStatus = Rune.DecodeFromUtf8(source.Slice(srcIdx), out Rune scalarValue, out int bytesConsumedJustNow);
                if (runeDecodeStatus != OperationStatus.Done)
                {
                    if (!isFinalBlock && runeDecodeStatus == OperationStatus.NeedMoreData)
                    {
                        goto NeedMoreData; // source ends in the middle of a multi-byte sequence
                    }

                    Debug.Assert(scalarValue == Rune.ReplacementChar); // DecodeFromUtfXX should've set replacement character on failure
                    goto MustEncodeNonAscii; // bad UTF-8 data seen
                }

                if (IsScalarValueAllowed(scalarValue))
                {
                    if (!scalarValue.TryEncodeToUtf8(destination.Slice(dstIdx), out int utf8CodeUnitCount))
                    {
                        goto DestTooSmall;
                    }
                    dstIdx += utf8CodeUnitCount;
                    srcIdx += utf8CodeUnitCount;
                    continue;
                }

            MustEncodeNonAscii:

                // At this point, we know we need to encode.

                int bytesWrittenJustNow = _scalarEscaper.EncodeUtf8(scalarValue, destination.Slice(dstIdx));
                if (bytesWrittenJustNow < 0)
                {
                    goto DestTooSmall;
                }

                dstIdx += bytesWrittenJustNow;
                srcIdx += bytesConsumedJustNow;
            }

            // And at this point, we're done!

            OperationStatus retVal = OperationStatus.Done;

        CommonReturn:
            bytesConsumed = srcIdx;
            bytesWritten = dstIdx;
            return retVal;

        DestTooSmall:
            retVal = OperationStatus.DestinationTooSmall;
            goto CommonReturn;

        NeedMoreData:
            retVal = OperationStatus.NeedMoreData;
            goto CommonReturn;
        }

        public int GetIndexOfFirstByteToEncode(ReadOnlySpan<byte> data)
        {
            int asciiBytesSkipped = 0;

#if NET
            // First, try calling the SIMD-enabled version.
            // The SIMD-enabled version handles only ASCII characters.
            asciiBytesSkipped = data.IndexOfAnyExcept(_allowedAsciiBytes);

            if ((uint)asciiBytesSkipped >= (uint)data.Length || UnicodeUtility.IsAsciiCodePoint(data[asciiBytesSkipped]))
            {
                // We either processed the whole span (in which case asciiBytesSkipped is -1), or we've found a disallowed ASCII byte.
                return asciiBytesSkipped;
            }
#endif

            int dataOriginalLength = data.Length;

            data = data.Slice(asciiBytesSkipped);

            // If there's any leftover data, try consuming it now.

            while (!data.IsEmpty)
            {
                OperationStatus opStatus = Rune.DecodeFromUtf8(data, out Rune scalarValue, out int bytesConsumed);
                if (opStatus != OperationStatus.Done) { break; } // bad data found, must escape
                if (bytesConsumed >= 4) { break; } // found supplementary code point, must escape

                UnicodeDebug.AssertIsBmpCodePoint((uint)scalarValue.Value);
                if (!_allowedBmpCodePoints.IsCharAllowed((char)scalarValue.Value)) { break; } // disallowed code point
                data = data.Slice(bytesConsumed);
            }

            return (data.IsEmpty) ? -1 : dataOriginalLength - data.Length;
        }

        public unsafe int GetIndexOfFirstCharToEncode(ReadOnlySpan<char> data)
        {
            int asciiCharsSkipped = 0;

#if NET
            // First, try calling the SIMD-enabled version.
            // The SIMD-enabled version handles only ASCII characters.
            if (data.Length >= 8)
            {
                asciiCharsSkipped = data.IndexOfAnyExcept(_allowedAsciiChars);

                if ((uint)asciiCharsSkipped >= (uint)data.Length || char.IsAscii(data[asciiCharsSkipped]))
                {
                    // We either processed the whole span (in which case asciiCharsSkipped is -1), or we've found a disallowed ASCII char.
                    return asciiCharsSkipped;
                }
            }
#endif

            fixed (char* pData = data)
            {
                nuint lengthInChars = (uint)data.Length;
                nuint idx = (uint)asciiCharsSkipped;

                // If there's any leftover data, try consuming it now.

                if (idx < lengthInChars)
                {
                    _AssertThisNotNull(); // hoist "this != null" check out of hot loop below

                    // unroll the loop 8x
                    nint loopIter = 0;
                    for (; lengthInChars - idx >= 8; idx += 8)
                    {
                        loopIter = -1;
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx + (nuint)(++loopIter)])) { goto BrokeInUnrolledLoop; }
                    }

                    for (; idx < lengthInChars; idx++)
                    {
                        if (!_allowedBmpCodePoints.IsCharAllowed(pData[idx])) { break; }
                    }

                    goto Return;

                BrokeInUnrolledLoop:
                    idx += (nuint)loopIter;
                }

            Return:

                Debug.Assert(0 <= idx && idx <= lengthInChars);
                int idx32 = (int)idx;
                if (idx32 == (int)lengthInChars)
                {
                    idx32 = -1;
                }
                return idx32;
            }
        }

        /// <summary>
        /// Given a scalar value, returns a value stating whether that value is present
        /// in this encoder's allow list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsScalarValueAllowed(Rune value)
        {
            return _allowedBmpCodePoints.IsCodePointAllowed((uint)value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AssertThisNotNull()
        {
            // Used for hoisting "'this' is not null" assertions outside hot loops.
            if (GetType() == typeof(OptimizedInboxTextEncoder)) { /* intentionally left blank */ }
        }
    }
}
