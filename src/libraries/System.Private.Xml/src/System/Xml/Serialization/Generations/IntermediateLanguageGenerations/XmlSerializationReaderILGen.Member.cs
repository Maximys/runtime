// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization.Mappings.AccessorMappings;

namespace System.Xml.Serialization.Generations.IntermediateLanguageGenerations
{
    internal sealed partial class XmlSerializationReaderILGen
    {
        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        private sealed class Member
        {
            private readonly bool _isArray;

            internal Member(string source, string? arrayName, int i, MemberMapping mapping)
                : this(source, null, arrayName, i, mapping, false, null)
            {
            }

            internal Member(string source, string? arrayName, int i, MemberMapping mapping, string? choiceSource)
                : this(source, null, arrayName, i, mapping, false, choiceSource)
            {
            }

            internal Member(string source, string? arraySource, string? arrayName, int i, MemberMapping mapping)
                : this(source, arraySource, arrayName, i, mapping, false, null)
            {
            }

            internal Member(string source, string? arraySource, string? arrayName, int i, MemberMapping mapping, string? choiceSource)
                : this(source, arraySource, arrayName, i, mapping, false, choiceSource)
            {
            }

            internal Member(string source, string? arrayName, int i, MemberMapping mapping, bool multiRef)
                : this(source, null, arrayName, i, mapping, multiRef, null)
            {
            }

            internal Member(string source, string? arraySource, string? arrayName, int i, MemberMapping mapping, bool multiRef, string? choiceSource)
            {
                Source = source;
                ArrayName = string.Create(CultureInfo.InvariantCulture, $"{arrayName}_{i}");
                ChoiceArrayName = $"choice_{ArrayName}";
                ChoiceSource = choiceSource;

                if (mapping.TypeDesc!.IsArrayLike)
                {
                    if (arraySource != null)
                    {
                        ArraySource = arraySource;
                    }
                    else
                    {
                        ArraySource = XmlSerializationReaderILGen.GetArraySource(mapping.TypeDesc, ArrayName, multiRef);
                    }
                    _isArray = mapping.TypeDesc.IsArray;
                    IsList = !_isArray;
                    if (mapping.ChoiceIdentifier != null)
                    {
                        ChoiceArraySource = XmlSerializationReaderILGen.GetArraySource(mapping.TypeDesc, ChoiceArrayName, multiRef);

                        string a = ChoiceArrayName;
                        string c = $"c{a}";
                        string choiceTypeFullName = mapping.ChoiceIdentifier.Mapping!.TypeDesc!.CSharpName;
                        string castString = $"({choiceTypeFullName}[])";

                        string init = $"{a} = {castString}EnsureArrayIndex({a}, {c}, {ReflectionAwareILGen.GetStringForTypeof(choiceTypeFullName)});";
                        ChoiceArraySource = init + ReflectionAwareILGen.GetStringForArrayMember(a, $"{c}++");
                    }
                    else
                    {
                        ChoiceArraySource = ChoiceSource;
                    }
                }
                else
                {
                    ArraySource = arraySource ?? source;
                    ChoiceArraySource = ChoiceSource;
                }

                Mapping = mapping;
            }

            internal MemberMapping Mapping { get; }

            internal string Source { get; }

            internal string ArrayName { get; }

            internal string ArraySource { get; }

            internal bool IsList { get; }

            internal bool IsArrayLike
            {
                get { return (_isArray || IsList); }
            }

            internal bool IsNullable { get; set; }

            internal int FixupIndex { get; set; } = -1;

            internal string? ParamsReadSource { get; set; }

            internal string? CheckSpecifiedSource { get; set; }

            internal string? ChoiceSource { get; }

            internal string ChoiceArrayName { get; }

            internal string? ChoiceArraySource { get; }
        }
    }
}
