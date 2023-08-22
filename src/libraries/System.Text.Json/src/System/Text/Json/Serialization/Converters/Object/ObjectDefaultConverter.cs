// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization.Converters
{
    /// <summary>
    /// Default base class implementation of <cref>JsonObjectConverter{T}</cref>.
    /// </summary>
    internal class ObjectDefaultConverter<T> : JsonObjectConverter<T> where T : notnull
    {
        private static readonly HashSet<MetadataPropertyName> MetadataPropertyNamesWhichShouldNotHaveNeighbors = new HashSet<MetadataPropertyName>
        {
            MetadataPropertyName.Ref
        };

        internal override bool CanHaveMetadata => true;
        internal override bool SupportsCreateObjectDelegate => true;

        internal override bool OnTryRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, scoped ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            JsonTypeInfo jsonTypeInfo = state.Current.JsonTypeInfo;

            if (!state.SupportContinuation && !state.Current.CanContainMetadata)
            {
                return TryReadByMetadataFreeAlgorithm(ref reader, jsonTypeInfo, options, ref state, out value);
            }
            else
            {
                return TryReadByMetadataAlgorithm(ref reader, jsonTypeInfo, options, ref state, out value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReadByMetadataAlgorithm(
            ref Utf8JsonReader reader,
            JsonTypeInfo jsonTypeInfo,
            JsonSerializerOptions options,
            scoped ref ReadStack state,
            [MaybeNullWhen(false)] out T value)
        {
            object obj;

            // Slower path that supports continuation and reading metadata.

            if (state.Current.ObjectState == StackFrameObjectState.None)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    ThrowHelper.ThrowJsonException_DeserializeUnableToConvertValue(Type);
                }

                state.Current.ObjectState = StackFrameObjectState.StartToken;
            }

            // Handle the metadata properties.
            ReadStack metadataState = state;
            Utf8JsonReader metadataReader = reader;
            if (metadataState.Current.CanContainMetadata && metadataState.Current.ObjectState < StackFrameObjectState.ReadMetadata)
            {
                try
                {
                    if (!JsonSerializer.TryReadMetadata(this, jsonTypeInfo, ref metadataReader, ref metadataState))
                    {
                        state = metadataState;
                        reader = metadataReader;
                        value = default;
                        return false;
                    }
                }
                catch
                {
                    state = metadataState;
                    reader = metadataReader;
                    throw;
                }

                if (metadataState.Current.MetadataPropertyNames == MetadataPropertyName.Ref)
                {
                    state = metadataState;
                    reader = metadataReader;
                    value = JsonSerializer.ResolveReferenceId<T>(ref state);
                    return true;
                }

                metadataState.Current.ObjectState = StackFrameObjectState.ReadMetadata;
                JsonSerializer.TryMoveToPropertyName(ref reader, ref state);
            }

            // Dispatch to any polymorphic converters: should always be entered regardless of ObjectState progress
            if ((metadataState.Current.MetadataPropertyNames & MetadataPropertyName.Type) != 0 &&
                metadataState.Current.PolymorphicSerializationState != PolymorphicSerializationState.PolymorphicReEntryStarted &&
                ResolvePolymorphicConverter(jsonTypeInfo, ref metadataState) is JsonConverter polymorphicConverter)
            {
                Debug.Assert(!IsValueType);
                bool success = polymorphicConverter.OnTryReadAsObject(ref metadataReader, polymorphicConverter.Type!, options, ref metadataState, out object? objectResult);
                value = (T)objectResult!;
                metadataState.ExitPolymorphicConverter(success);
                state = metadataState;
                reader = metadataReader;
                return success;
            }

            if (metadataState.Current.ObjectState < StackFrameObjectState.CreatedObject)
            {
                if (metadataState.Current.CanContainMetadata)
                {
                    JsonSerializer.ValidateMetadataForObjectConverter(ref metadataState);
                }

                if (metadataState.Current.MetadataPropertyNames == MetadataPropertyName.Ref)
                {
                    value = JsonSerializer.ResolveReferenceId<T>(ref metadataState);
                    return true;
                }

                if (metadataState.ParentProperty?.TryGetPrePopulatedValue(ref metadataState) == true)
                {
                    obj = metadataState.Current.ReturnValue!;
                }
                else
                {
                    if (jsonTypeInfo.CreateObject == null)
                    {
                        ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(jsonTypeInfo.Type, ref metadataReader, ref metadataState);
                    }

                    obj = jsonTypeInfo.CreateObject()!;
                }

                if ((metadataState.Current.MetadataPropertyNames & MetadataPropertyName.Id) != 0)
                {
                    Debug.Assert(metadataState.Metadata.ReferenceId != null);
                    Debug.Assert(options.ReferenceHandlingStrategy == ReferenceHandlingStrategy.Preserve);
                    state.ReferenceResolver.AddReference(metadataState.Metadata.ReferenceId, obj);
                    state.Metadata.ReferenceId = null;
                }

                jsonTypeInfo.OnDeserializing?.Invoke(obj);

                state.Current.ReturnValue = obj;
                state.Current.IsPopulating = metadataState.Current.IsPopulating;
                state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                state.Current.InitializeRequiredPropertiesValidationState(jsonTypeInfo);
            }
            else
            {
                obj = state.Current.ReturnValue!;
                Debug.Assert(obj != null);
            }

            if (PopulatePropertiesSlowPath(obj, options, ref reader, ref state))
            {
                jsonTypeInfo.OnDeserialized?.Invoke(obj);
                state.Current.ValidateAllRequiredPropertiesAreRead(jsonTypeInfo);

                // Unbox
                Debug.Assert(obj != null);
                value = (T)obj;

                // Check if we are trying to build the sorted cache.
                if (state.Current.PropertyRefCache != null)
                {
                    jsonTypeInfo.UpdateSortedPropertyCache(ref state.Current);
                }

                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReadByMetadataFreeAlgorithm(
            ref Utf8JsonReader reader,
            JsonTypeInfo jsonTypeInfo,
            JsonSerializerOptions options,
            scoped ref ReadStack state,
            [MaybeNullWhen(false)] out T value)
        {
            object obj;

            // Fast path that avoids maintaining state variables and dealing with preserved references.

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                ThrowHelper.ThrowJsonException_DeserializeUnableToConvertValue(Type);
            }

            if (state.ParentProperty?.TryGetPrePopulatedValue(ref state) == true)
            {
                obj = state.Current.ReturnValue!;
            }
            else
            {
                if (jsonTypeInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(jsonTypeInfo.Type, ref reader, ref state);
                }

                obj = jsonTypeInfo.CreateObject()!;
            }

            PopulatePropertiesFastPath(obj, jsonTypeInfo, options, ref reader, ref state);
            Debug.Assert(obj != null);
            value = (T)obj;
            return true;
        }

        // This method is using aggressive inlining to avoid extra stack frame for deep object graphs.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PopulatePropertiesFastPath(object obj, JsonTypeInfo jsonTypeInfo, JsonSerializerOptions options, ref Utf8JsonReader reader, scoped ref ReadStack state)
        {
            jsonTypeInfo.OnDeserializing?.Invoke(obj);
            state.Current.InitializeRequiredPropertiesValidationState(jsonTypeInfo);

            // Process all properties.
            while (true)
            {
                // Read the property name or EndObject.
                reader.ReadWithVerify();

                JsonTokenType tokenType = reader.TokenType;

                if (tokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                // Read method would have thrown if otherwise.
                Debug.Assert(tokenType == JsonTokenType.PropertyName);

                ReadOnlySpan<byte> unescapedPropertyName = JsonSerializer.GetPropertyName(ref reader);
                JsonPropertyInfo jsonPropertyInfo = JsonSerializer.LookupProperty(
                    obj,
                    unescapedPropertyName,
                    ref state,
                    options,
                    out bool useExtensionProperty);

                ReadPropertyValue(obj, ref state, ref reader, jsonPropertyInfo, useExtensionProperty);
            }

            jsonTypeInfo.OnDeserialized?.Invoke(obj);
            state.Current.ValidateAllRequiredPropertiesAreRead(jsonTypeInfo);

            // Check if we are trying to build the sorted cache.
            if (state.Current.PropertyRefCache != null)
            {
                jsonTypeInfo.UpdateSortedPropertyCache(ref state.Current);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool PopulatePropertiesSlowPath(object obj, JsonSerializerOptions options, ref Utf8JsonReader reader, scoped ref ReadStack state)
        {
            while (true)
            {
                if (!JsonSerializer.TryMoveToPropertyName(ref reader, ref state))
                {
                    // The read-ahead functionality will do the Read().
                    state.Current.ReturnValue = obj;
                    return false;
                }

                JsonPropertyInfo jsonPropertyInfo;

                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;

                    JsonTokenType tokenType = reader.TokenType;
                    if (tokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    // Read method would have thrown if otherwise.
                    Debug.Assert(tokenType == JsonTokenType.PropertyName);

                    ReadOnlySpan<byte> unescapedPropertyName = GetPropertyName(ref state, ref reader);
                    jsonPropertyInfo = JsonSerializer.LookupProperty(
                        obj,
                        unescapedPropertyName,
                        ref state,
                        options,
                        out bool useExtensionProperty);

                    state.Current.UseExtensionProperty = useExtensionProperty;
                }
                else
                {
                    Debug.Assert(state.Current.JsonPropertyInfo != null);
                    jsonPropertyInfo = state.Current.JsonPropertyInfo!;
                }

                if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                {
                    if (!jsonPropertyInfo.CanDeserializeOrPopulate)
                    {
                        if (!reader.TrySkip())
                        {
                            state.Current.ReturnValue = obj;
                            return false;
                        }

                        state.Current.EndProperty();
                        continue;
                    }

                    if (!ReadAheadPropertyValue(ref state, ref reader, jsonPropertyInfo))
                    {
                        state.Current.ReturnValue = obj;
                        return false;
                    }
                }

                if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                {
                    // Obtain the CLR value from the JSON and set the member.
                    if (!state.Current.UseExtensionProperty)
                    {
                        if (!jsonPropertyInfo.ReadJsonAndSetMember(obj, ref state, ref reader))
                        {
                            state.Current.ReturnValue = obj;
                            return false;
                        }
                    }
                    else
                    {
                        if (!jsonPropertyInfo.ReadJsonAndAddExtensionProperty(obj, ref state, ref reader))
                        {
                            // No need to set 'value' here since JsonElement must be read in full.
                            state.Current.ReturnValue = obj;
                            return false;
                        }
                    }

                    state.Current.EndProperty();
                }
            }

            return true;
        }

        internal sealed override bool OnTryWrite(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            JsonTypeInfo jsonTypeInfo = state.Current.JsonTypeInfo;
            jsonTypeInfo.ValidateCanBeUsedForPropertyMetadataSerialization();

            object obj = value; // box once

            if (!state.SupportContinuation)
            {
                writer.WriteStartObject();

                if (state.CurrentContainsMetadata && CanHaveMetadata)
                {
                    JsonSerializer.WriteMetadataForObject(this, ref state, writer);
                }

                jsonTypeInfo.OnSerializing?.Invoke(obj);

                List<KeyValuePair<string, JsonPropertyInfo>> properties = jsonTypeInfo.PropertyCache!.List;
                for (int i = 0; i < properties.Count; i++)
                {
                    JsonPropertyInfo jsonPropertyInfo = properties[i].Value;
                    if (jsonPropertyInfo.CanSerialize)
                    {
                        // Remember the current property for JsonPath support if an exception is thrown.
                        state.Current.JsonPropertyInfo = jsonPropertyInfo;
                        state.Current.NumberHandling = jsonPropertyInfo.EffectiveNumberHandling;

                        bool success = jsonPropertyInfo.GetMemberAndWriteJson(obj, ref state, writer);
                        // Converters only return 'false' when out of data which is not possible in fast path.
                        Debug.Assert(success);

                        state.Current.EndProperty();
                    }
                }

                // Write extension data after the normal properties.
                JsonPropertyInfo? extensionDataProperty = jsonTypeInfo.ExtensionDataProperty;
                if (extensionDataProperty?.CanSerialize == true)
                {
                    // Remember the current property for JsonPath support if an exception is thrown.
                    state.Current.JsonPropertyInfo = extensionDataProperty;
                    state.Current.NumberHandling = extensionDataProperty.EffectiveNumberHandling;

                    bool success = extensionDataProperty.GetMemberAndWriteJsonExtensionData(obj, ref state, writer);
                    Debug.Assert(success);

                    state.Current.EndProperty();
                }

                writer.WriteEndObject();
            }
            else
            {
                if (!state.Current.ProcessedStartToken)
                {
                    writer.WriteStartObject();

                    if (state.CurrentContainsMetadata && CanHaveMetadata)
                    {
                        JsonSerializer.WriteMetadataForObject(this, ref state, writer);
                    }

                    jsonTypeInfo.OnSerializing?.Invoke(obj);

                    state.Current.ProcessedStartToken = true;
                }

                List<KeyValuePair<string, JsonPropertyInfo>> propertyList = jsonTypeInfo.PropertyCache!.List;
                while (state.Current.EnumeratorIndex < propertyList.Count)
                {
                    JsonPropertyInfo jsonPropertyInfo = propertyList[state.Current.EnumeratorIndex].Value;
                    if (jsonPropertyInfo.CanSerialize)
                    {
                        state.Current.JsonPropertyInfo = jsonPropertyInfo;
                        state.Current.NumberHandling = jsonPropertyInfo.EffectiveNumberHandling;

                        if (!jsonPropertyInfo.GetMemberAndWriteJson(obj!, ref state, writer))
                        {
                            Debug.Assert(jsonPropertyInfo.EffectiveConverter.ConverterStrategy != ConverterStrategy.Value);
                            return false;
                        }

                        state.Current.EndProperty();
                        state.Current.EnumeratorIndex++;

                        if (ShouldFlush(writer, ref state))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        state.Current.EnumeratorIndex++;
                    }
                }

                // Write extension data after the normal properties.
                if (state.Current.EnumeratorIndex == propertyList.Count)
                {
                    JsonPropertyInfo? extensionDataProperty = jsonTypeInfo.ExtensionDataProperty;
                    if (extensionDataProperty?.CanSerialize == true)
                    {
                        // Remember the current property for JsonPath support if an exception is thrown.
                        state.Current.JsonPropertyInfo = extensionDataProperty;
                        state.Current.NumberHandling = extensionDataProperty.EffectiveNumberHandling;

                        if (!extensionDataProperty.GetMemberAndWriteJsonExtensionData(obj, ref state, writer))
                        {
                            return false;
                        }

                        state.Current.EndProperty();
                        state.Current.EnumeratorIndex++;

                        if (ShouldFlush(writer, ref state))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        state.Current.EnumeratorIndex++;
                    }
                }

                if (!state.Current.ProcessedEndToken)
                {
                    state.Current.ProcessedEndToken = true;
                    writer.WriteEndObject();
                }
            }

            jsonTypeInfo.OnSerialized?.Invoke(obj);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> GetPropertyName(
            scoped ref ReadStack state,
            ref Utf8JsonReader reader)
        {
            ReadOnlySpan<byte> propertyName = reader.GetSpan();

            MetadataPropertyName name = JsonSerializer.GetMetadataPropertyName(propertyName, resolver: null);
            switch (name)
            {
                case MetadataPropertyName.Id
                    when state.Current.State != ReadStates.None:
                    ThrowHelper.ThrowUnexpectedMetadataException(propertyName, ref reader, ref state);
                    break;
                default:
                    if (MetadataPropertyNamesWhichShouldNotHaveNeighbors.Contains(name))
                    {
                        ThrowHelper.ThrowUnexpectedMetadataException(propertyName, ref reader, ref state);
                    }
                    break;
            }

            state.Current.State |= (ReadStates)name;
            return JsonSerializer.GetPropertyName(ref reader);
        }

        // AggressiveInlining since this method is only called from two locations and is on a hot path.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void ReadPropertyValue(
            object obj,
            scoped ref ReadStack state,
            ref Utf8JsonReader reader,
            JsonPropertyInfo jsonPropertyInfo,
            bool useExtensionProperty)
        {
            // Skip the property if not found.
            if (!jsonPropertyInfo.CanDeserializeOrPopulate)
            {
                // The Utf8JsonReader.Skip() method will fail fast if it detects that we're reading
                // from a partially read buffer, regardless of whether the next value is available.
                // This can result in erroneous failures in cases where a custom converter is calling
                // into a built-in converter (cf. https://github.com/dotnet/runtime/issues/74108).
                // For this reason we need to call the TrySkip() method instead -- the serializer
                // should guarantee sufficient read-ahead has been performed for the current object.
                bool success = reader.TrySkip();
                Debug.Assert(success, "Serializer should guarantee sufficient read-ahead has been done.");
            }
            else
            {
                // Set the property value.
                reader.ReadWithVerify();

                if (!useExtensionProperty)
                {
                    jsonPropertyInfo.ReadJsonAndSetMember(obj, ref state, ref reader);
                }
                else
                {
                    jsonPropertyInfo.ReadJsonAndAddExtensionProperty(obj, ref state, ref reader);
                }
            }

            // Ensure any exception thrown in the next read does not have a property in its JsonPath.
            state.Current.EndProperty();
        }

        protected static bool ReadAheadPropertyValue(scoped ref ReadStack state, ref Utf8JsonReader reader, JsonPropertyInfo jsonPropertyInfo)
        {
            // Returning false below will cause the read-ahead functionality to finish the read.
            state.Current.PropertyState = StackFramePropertyState.ReadValue;

            if (!state.Current.UseExtensionProperty)
            {
                if (!SingleValueReadWithReadAhead(jsonPropertyInfo.EffectiveConverter.RequiresReadAhead, ref reader, ref state))
                {
                    return false;
                }
            }
            else
            {
                // The actual converter is JsonElement, so force a read-ahead.
                if (!SingleValueReadWithReadAhead(requiresReadAhead: true, ref reader, ref state))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
