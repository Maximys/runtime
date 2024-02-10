// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization.Mappings.Navigation;
using System.Xml.Serialization.Mappings.TypeMappings;
using System.Xml.Serialization.Types;

namespace System.Xml.Serialization
{
    // These classes provide a higher level view on reflection specific to
    // Xml serialization, for example:
    // - allowing one to talk about types w/o having them compiled yet
    // - abstracting collections & arrays
    // - abstracting classes, structs, interfaces
    // - knowing about XSD primitives
    // - dealing with Serializable and xmlelement
    // and lots of other little details

    internal sealed class TypeScope
    {
        internal const string BooleanFormatterName = "Boolean";
        internal const string ByteArrayBase64FormatterName = "ByteArrayBase64";
        internal const string ByteArrayHexFormatterName = "ByteArrayHex";
        internal const string ByteFormatterName = "Byte";
        internal const string CharFormatterName = "Char";
        internal const string DateFormatterName = "Date";
        internal const string DateTimeFormatterName = "DateTime";
        internal const string DateTimeOffsetFormatterName = "DateTimeOffset";
        internal const string DecimalFormatterName = "Decimal";
        internal const string DoubleFormatterName = "Double";
        internal const string GuidFormatterName = "Guid";
        internal const string Int16FormatterName = "Int16";
        internal const string Int32FormatterName = "Int32";
        internal const string Int64FormatterName = "Int64";
        internal const string NoncolonizedNameFormatterName = "XmlNCName";
        internal const string SByteFormatterName = "SByte";
        internal const string SingleFormatterName = "Single";
        internal const string StringFormatterName = "String";
        internal const string TimeFormatterName = "Time";
        internal const string TimeSpanFormatterName = "TimeSpan";
        internal const string UInt16FormatterName = "UInt16";
        internal const string UInt32FormatterName = "UInt32";
        internal const string UInt64FormatterName = "UInt64";
        internal const string UnsupportedFormatterName = "String";
        internal const string XmlNameFormatterName = "XmlName";
        internal const string XmlNmTokenFormatterName = "XmlNmToken";
        internal const string XmlNmTokensFormatterName = "XmlNmTokens";
        internal const string XmlQualifiedNameFormatterName = "XmlQualifiedName";

        private const TypeFlags BaseFlags = TypeFlags.CanBeAttributeValue
            | TypeFlags.CanBeElementValue;
        private const TypeFlags WithoutDirectMappingFlags = TypeFlags.AmbiguousDataType
            | BaseFlags;

        private readonly Dictionary<Type, TypeDesc> _typeDescs = new Dictionary<Type, TypeDesc>();
        private readonly Dictionary<Type, TypeDesc> _arrayTypeDescs = new Dictionary<Type, TypeDesc>();
        private readonly List<TypeMapping> _typeMappings = new List<TypeMapping>();

        private static readonly Dictionary<Type, TypeDesc> s_primitiveTypes = new Dictionary<Type, TypeDesc>();
        private static readonly Dictionary<XmlSchemaSimpleType, TypeDesc> s_primitiveDataTypes = new Dictionary<XmlSchemaSimpleType, TypeDesc>();
        private static readonly NavigationNameTable<TypeDesc> s_primitiveNames = new NavigationNameTable<TypeDesc>();

        private static readonly string[] s_unsupportedTypes = new string[] {
            DataTypeNames.AnyUri,
            DataTypeNames.Duration,
            DataTypeNames.Entity,
            DataTypeNames.Entities,
            DataTypeNames.GDay,
            DataTypeNames.GMonth,
            DataTypeNames.GMonthDay,
            DataTypeNames.GYear,
            DataTypeNames.GYearMonth,
            DataTypeNames.Id,
            DataTypeNames.IdRef,
            DataTypeNames.IdRefs,
            DataTypeNames.Integer,
            DataTypeNames.Language,
            DataTypeNames.NegativeInteger,
            DataTypeNames.NonNegativeInteger,
            DataTypeNames.NonPositiveInteger,
            //"normalizedString",
            DataTypeNames.Notation,
            DataTypeNames.PositiveInteger,
            DataTypeNames.Token
        };

        static TypeScope()
        {
            AddPrimitive(typeof(string), DataTypeNames.String, StringFormatterName, BaseFlags | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            AddPrimitive(typeof(int), DataTypeNames.Int32, Int32FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(bool), DataTypeNames.Boolean, BooleanFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(short), DataTypeNames.Int16, Int16FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(long), DataTypeNames.Int64, Int64FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(float), DataTypeNames.Single, SingleFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(double), DataTypeNames.Double, DoubleFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(decimal), DataTypeNames.Decimal, DecimalFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(DateTime), DataTypeNames.DateTime, DateTimeFormatterName, BaseFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(XmlQualifiedName), DataTypeNames.XmlQualifiedName, XmlQualifiedNameFormatterName, TypeFlags.CanBeAttributeValue | TypeFlags.HasCustomFormatter | TypeFlags.HasIsEmpty | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired | TypeFlags.Reference);
            AddPrimitive(typeof(byte), DataTypeNames.Byte, ByteFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(sbyte), DataTypeNames.SByte, SByteFormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(ushort), DataTypeNames.UInt16, UInt16FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(uint), DataTypeNames.UInt32, UInt32FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(ulong), DataTypeNames.UInt64, UInt64FormatterName, BaseFlags | TypeFlags.XmlEncodingNotRequired);

            // Types without direct mapping (ambiguous)
            AddPrimitive(typeof(DateTime), DataTypeNames.Date, DateFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddPrimitive(typeof(DateTime), DataTypeNames.Time, TimeFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);

            AddPrimitive(typeof(string), DataTypeNames.XmlName, XmlNameFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), DataTypeNames.NoncolonizedName, NoncolonizedNameFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), DataTypeNames.XmlNmToken, XmlNmTokenFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddPrimitive(typeof(string), DataTypeNames.XmlNmTokens, XmlNmTokensFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);

            AddPrimitive(typeof(byte[]), DataTypeNames.ByteArrayBase64, ByteArrayBase64FormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor);
            AddPrimitive(typeof(byte[]), DataTypeNames.ByteArrayHex, ByteArrayHexFormatterName, WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired | TypeFlags.HasDefaultConstructor);
            // NOTE, Micorosft: byte[] can also be used to mean array of bytes. That datatype is not a primitive, so we
            // can't use the AmbiguousDataType mechanism. To get an array of bytes in literal XML, apply [XmlArray] or
            // [XmlArrayItem].

            XmlSchemaPatternFacet guidPattern = new XmlSchemaPatternFacet();
            guidPattern.Value = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

            AddNonXsdPrimitive(typeof(Guid), DataTypeNames.Guid, UrtTypes.Namespace, GuidFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), new XmlSchemaFacet[] { guidPattern }, BaseFlags | TypeFlags.XmlEncodingNotRequired | TypeFlags.IgnoreDefault);
            AddNonXsdPrimitive(typeof(char), DataTypeNames.Char, UrtTypes.Namespace, CharFormatterName, new XmlQualifiedName("unsignedShort", XmlSchema.Namespace), Array.Empty<XmlSchemaFacet>(), BaseFlags | TypeFlags.HasCustomFormatter | TypeFlags.IgnoreDefault);
            AddNonXsdPrimitive(typeof(TimeSpan), DataTypeNames.TimeSpan, UrtTypes.Namespace, TimeSpanFormatterName, new XmlQualifiedName("duration", XmlSchema.Namespace), Array.Empty<XmlSchemaFacet>(), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddNonXsdPrimitive(typeof(DateTimeOffset), DataTypeNames.DateTimeOffset, UrtTypes.Namespace, DateTimeOffsetFormatterName, new XmlQualifiedName("dateTime", XmlSchema.Namespace), Array.Empty<XmlSchemaFacet>(), BaseFlags | TypeFlags.XmlEncodingNotRequired);

            AddSoapEncodedTypes(Soap.Encoding);

            // Unsupported types that we map to string, if in the future we decide
            // to add support for them we would need to create custom formatters for them
            // normalizedString is the only one unsupported type that suppose to preserve whitespace
            AddPrimitive(typeof(string), DataTypeNames.NormalizedString, UnsupportedFormatterName, WithoutDirectMappingFlags | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            for (int i = 0; i < s_unsupportedTypes.Length; i++)
            {
                AddPrimitive(typeof(string), s_unsupportedTypes[i], UnsupportedFormatterName, WithoutDirectMappingFlags | TypeFlags.CanBeTextValue | TypeFlags.Reference | TypeFlags.CollapseWhitespace);
            }
        }

        internal static bool IsKnownType(Type type)
        {
            if (type == typeof(object))
                return true;
            if (type.IsEnum)
                return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String: return true;
                case TypeCode.Int32: return true;
                case TypeCode.Boolean: return true;
                case TypeCode.Int16: return true;
                case TypeCode.Int64: return true;
                case TypeCode.Single: return true;
                case TypeCode.Double: return true;
                case TypeCode.Decimal: return true;
                case TypeCode.DateTime: return true;
                case TypeCode.Byte: return true;
                case TypeCode.SByte: return true;
                case TypeCode.UInt16: return true;
                case TypeCode.UInt32: return true;
                case TypeCode.UInt64: return true;
                case TypeCode.Char: return true;
                default:
                    if (type == typeof(XmlQualifiedName))
                        return true;
                    else if (type == typeof(byte[]))
                        return true;
                    else if (type == typeof(Guid))
                        return true;
                    else if (type == typeof(TimeSpan))
                        return true;
                    else if (type == typeof(DateTimeOffset))
                        return true;
                    else if (type == typeof(XmlNode[]))
                        return true;
                    break;
            }
            return false;
        }

        private static void AddSoapEncodedTypes(string ns)
        {
            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.NormalizedString, ns, UnsupportedFormatterName, new XmlQualifiedName("normalizedString", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.Reference | TypeFlags.HasDefaultConstructor);
            for (int i = 0; i < s_unsupportedTypes.Length; i++)
            {
                AddSoapEncodedPrimitive(typeof(string), s_unsupportedTypes[i], ns, UnsupportedFormatterName, new XmlQualifiedName(s_unsupportedTypes[i], XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.Reference | TypeFlags.CollapseWhitespace);
            }

            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.String, ns, StringFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.CanBeTextValue | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(int), DataTypeNames.Int32, ns, Int32FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(bool), DataTypeNames.Boolean, ns, BooleanFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(short), DataTypeNames.Int16, ns, Int16FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(long), DataTypeNames.Int64, ns, Int64FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(float), DataTypeNames.Single, ns, SingleFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(double), DataTypeNames.Double, ns, DoubleFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(decimal), DataTypeNames.Decimal, ns, DecimalFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(DateTime), DataTypeNames.DateTime, ns, DateTimeFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(XmlQualifiedName), DataTypeNames.XmlQualifiedName, ns, XmlQualifiedNameFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.HasCustomFormatter | TypeFlags.HasIsEmpty | TypeFlags.CanBeElementValue | TypeFlags.XmlEncodingNotRequired | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(byte), DataTypeNames.Byte, ns, ByteFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(sbyte), DataTypeNames.SByte, ns, SByteFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(ushort), DataTypeNames.UInt16, ns, UInt16FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(uint), DataTypeNames.UInt32, ns, UInt32FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(ulong), DataTypeNames.UInt64, ns, UInt64FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), BaseFlags | TypeFlags.XmlEncodingNotRequired);

            // Types without direct mapping (ambiguous)
            AddSoapEncodedPrimitive(typeof(DateTime), DataTypeNames.Date, ns, DateFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(DateTime), DataTypeNames.Time, ns, TimeFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.XmlEncodingNotRequired);

            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.XmlName, ns, XmlNameFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.NoncolonizedName, ns, NoncolonizedNameFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.XmlNmToken, ns, XmlNmTokenFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);
            AddSoapEncodedPrimitive(typeof(string), DataTypeNames.XmlNmTokens, ns, XmlNmTokensFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference);

            AddSoapEncodedPrimitive(typeof(byte[]), DataTypeNames.ByteArrayBase64, ns, ByteArrayBase64FormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired);
            AddSoapEncodedPrimitive(typeof(byte[]), DataTypeNames.ByteArrayHex, ns, ByteArrayHexFormatterName, new XmlQualifiedName("string", XmlSchema.Namespace), WithoutDirectMappingFlags | TypeFlags.HasCustomFormatter | TypeFlags.Reference | TypeFlags.IgnoreDefault | TypeFlags.XmlEncodingNotRequired);

            AddSoapEncodedPrimitive(typeof(string), "arrayCoordinate", ns, "String", new XmlQualifiedName("string", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue);
            AddSoapEncodedPrimitive(typeof(byte[]), DataTypeNames.Base64, ns, "ByteArrayBase64", new XmlQualifiedName("base64Binary", XmlSchema.Namespace), TypeFlags.CanBeAttributeValue | TypeFlags.CanBeElementValue | TypeFlags.IgnoreDefault | TypeFlags.Reference);
        }

        private static void AddPrimitive(Type type, string dataTypeName, string formatterName, TypeFlags flags)
        {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType();
            dataType.Name = dataTypeName;
            TypeDesc typeDesc = new TypeDesc(type, true, dataType, formatterName, flags);
            s_primitiveTypes.TryAdd(type, typeDesc);
            s_primitiveDataTypes.Add(dataType, typeDesc);
            s_primitiveNames.Add(dataTypeName, XmlSchema.Namespace, typeDesc);
        }

        private static void AddNonXsdPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, XmlSchemaFacet[] facets, TypeFlags flags)
        {
            XmlSchemaSimpleType dataType = new XmlSchemaSimpleType();
            dataType.Name = dataTypeName;
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
            restriction.BaseTypeName = baseTypeName;
            foreach (XmlSchemaFacet facet in facets)
            {
                restriction.Facets.Add(facet);
            }
            dataType.Content = restriction;
            TypeDesc typeDesc = new TypeDesc(type, false, dataType, formatterName, flags);
            s_primitiveTypes.TryAdd(type, typeDesc);
            s_primitiveDataTypes.Add(dataType, typeDesc);
            s_primitiveNames.Add(dataTypeName, ns, typeDesc);
        }

        private static void AddSoapEncodedPrimitive(Type type, string dataTypeName, string ns, string formatterName, XmlQualifiedName baseTypeName, TypeFlags flags)
        {
            AddNonXsdPrimitive(type, dataTypeName, ns, formatterName, baseTypeName, Array.Empty<XmlSchemaFacet>(), flags);
        }

        internal static TypeDesc? GetTypeDesc(string name, string ns)
        {
            return GetTypeDesc(name, ns, TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue | TypeFlags.CanBeAttributeValue);
        }

        internal static TypeDesc? GetTypeDesc(string name, string? ns, TypeFlags flags)
        {
            TypeDesc? typeDesc = s_primitiveNames[name, ns];
            if (typeDesc != null)
            {
                if ((typeDesc.Flags & flags) != 0)
                {
                    return typeDesc;
                }
            }
            return null;
        }

        internal static TypeDesc? GetTypeDesc(XmlSchemaSimpleType dataType)
        {
            TypeDesc? returnValue;

            s_primitiveDataTypes.TryGetValue(dataType, out returnValue);
            return returnValue;
        }

        [RequiresUnreferencedCode("calls GetTypeDesc")]
        internal TypeDesc GetTypeDesc(Type type)
        {
            return GetTypeDesc(type, null, true, true);
        }

        [RequiresUnreferencedCode("calls GetTypeDesc")]
        internal TypeDesc GetTypeDesc(Type type, MemberInfo? source, bool directReference)
        {
            return GetTypeDesc(type, source, directReference, true);
        }

        [RequiresUnreferencedCode("calls ImportTypeDesc")]
        internal TypeDesc GetTypeDesc(Type type, MemberInfo? source, bool directReference, bool throwOnError)
        {
            TypeDesc? returnValue;

            if (type.ContainsGenericParameters)
            {
                throw new InvalidOperationException(SR.Format(SR.XmlUnsupportedOpenGenericType, type));
            }

            if (!s_primitiveTypes.TryGetValue(type, out returnValue))
            {
                if (!_typeDescs.TryGetValue(type, out returnValue))
                {
                    returnValue = ImportTypeDesc(type, source, directReference);
                }
            }

            if (throwOnError)
            {
                returnValue.CheckSupported();
            }

            return returnValue;
        }

        [RequiresUnreferencedCode("calls ImportTypeDesc")]
        internal TypeDesc GetArrayTypeDesc(Type type)
        {
            TypeDesc? returnValue;

            if (!_arrayTypeDescs.TryGetValue(type, out returnValue))
            {
                TypeDesc typeDesc = GetTypeDesc(type);
                if (!typeDesc.IsArrayLike)
                {
                    typeDesc = ImportTypeDesc(type, null, false);
                }
                typeDesc.CheckSupported();
                _arrayTypeDescs.Add(type, typeDesc);
                returnValue = typeDesc;
            }

            return returnValue;
        }

        internal TypeMapping? GetTypeMappingFromTypeDesc(TypeDesc typeDesc)
        {
            foreach (TypeMapping typeMapping in TypeMappings)
            {
                if (typeMapping.TypeDesc == typeDesc)
                    return typeMapping;
            }
            return null;
        }

        internal Type? GetTypeFromTypeDesc(TypeDesc typeDesc)
        {
            if (typeDesc.Type != null)
            {
                return typeDesc.Type;
            }
            foreach (KeyValuePair<Type, TypeDesc> de in _typeDescs)
            {
                if (de.Value == typeDesc)
                {
                    return de.Key;
                }
            }
            return null;
        }

        [RequiresUnreferencedCode("calls GetEnumeratorElementType")]
        private TypeDesc ImportTypeDesc(Type type, MemberInfo? memberInfo, bool directReference)
        {
            TypeDesc? typeDesc;
            TypeKind kind;
            Type? arrayElementType = null;
            Type? baseType = null;
            TypeFlags flags = 0;
            Exception? exception = null;

            if (!type.IsVisible)
            {
                flags |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(SR.Format(SR.XmlTypeInaccessible, type.FullName));
            }
            else if (directReference && (type.IsAbstract && type.IsSealed))
            {
                flags |= TypeFlags.Unsupported;
                exception = new InvalidOperationException(SR.Format(SR.XmlTypeStatic, type.FullName));
            }
            if (DynamicAssemblies.IsTypeDynamic(type))
            {
                flags |= TypeFlags.UseReflection;
            }
            if (!type.IsValueType)
                flags |= TypeFlags.Reference;

            if (type == typeof(object))
            {
                kind = TypeKind.Root;
                flags |= TypeFlags.HasDefaultConstructor;
            }
            else if (type == typeof(ValueType))
            {
                kind = TypeKind.Enum;
                flags |= TypeFlags.Unsupported;
                exception ??= new NotSupportedException(SR.Format(SR.XmlSerializerUnsupportedType, type.FullName));
            }
            else if (type == typeof(void))
            {
                kind = TypeKind.Void;
            }
            else if (typeof(IXmlSerializable).IsAssignableFrom(type))
            {
                kind = TypeKind.Serializable;
                flags |= TypeFlags.Special | TypeFlags.CanBeElementValue;
                flags |= GetConstructorFlags(type);
            }
            else if (type.IsArray)
            {
                kind = TypeKind.Array;
                if (type.GetArrayRank() > 1)
                {
                    flags |= TypeFlags.Unsupported;
                    exception ??= new NotSupportedException(SR.Format(SR.XmlUnsupportedRank, type.FullName));
                }
                arrayElementType = type.GetElementType();
                flags |= TypeFlags.HasDefaultConstructor;
            }
            else if (typeof(ICollection).IsAssignableFrom(type) && !IsArraySegment(type))
            {
                kind = TypeKind.Collection;
                arrayElementType = GetCollectionElementType(type, memberInfo == null ? null : $"{memberInfo.DeclaringType!.FullName}.{memberInfo.Name}");
                flags |= GetConstructorFlags(type);
            }
            else if (type == typeof(XmlQualifiedName))
            {
                kind = TypeKind.Primitive;
            }
            else if (type.IsPrimitive)
            {
                kind = TypeKind.Primitive;
                flags |= TypeFlags.Unsupported;
                exception ??= new NotSupportedException(SR.Format(SR.XmlSerializerUnsupportedType, type.FullName));
            }
            else if (type.IsEnum)
            {
                kind = TypeKind.Enum;
            }
            else if (type.IsValueType)
            {
                kind = TypeKind.Struct;
                if (IsOptionalValue(type))
                {
                    baseType = type.GetGenericArguments()[0];
                    flags |= TypeFlags.OptionalValue;
                }
                else
                {
                    baseType = type.BaseType;
                }
                if (type.IsAbstract) flags |= TypeFlags.Abstract;
            }
            else if (type.IsClass)
            {
                if (type == typeof(XmlAttribute))
                {
                    kind = TypeKind.Attribute;
                    flags |= TypeFlags.Special | TypeFlags.CanBeAttributeValue;
                }
                else if (typeof(XmlNode).IsAssignableFrom(type))
                {
                    kind = TypeKind.Node;
                    baseType = type.BaseType;
                    flags |= TypeFlags.Special | TypeFlags.CanBeElementValue | TypeFlags.CanBeTextValue;
                    if (typeof(XmlText).IsAssignableFrom(type))
                        flags &= ~TypeFlags.CanBeElementValue;
                    else if (typeof(XmlElement).IsAssignableFrom(type))
                        flags &= ~TypeFlags.CanBeTextValue;
                    else if (type.IsAssignableFrom(typeof(XmlAttribute)))
                        flags |= TypeFlags.CanBeAttributeValue;
                }
                else
                {
                    kind = TypeKind.Class;
                    baseType = type.BaseType;
                    if (type.IsAbstract)
                        flags |= TypeFlags.Abstract;
                }
            }
            else if (type.IsInterface)
            {
                kind = TypeKind.Void;
                flags |= TypeFlags.Unsupported;
                if (exception == null)
                {
                    if (memberInfo == null)
                    {
                        exception = new NotSupportedException(SR.Format(SR.XmlUnsupportedInterface, type.FullName));
                    }
                    else
                    {
                        exception = new NotSupportedException(SR.Format(SR.XmlUnsupportedInterfaceDetails, $"{memberInfo.DeclaringType!.FullName}.{memberInfo.Name}", type.FullName));
                    }
                }
            }
            else
            {
                kind = TypeKind.Void;
                flags |= TypeFlags.Unsupported;
                exception ??= new NotSupportedException(SR.Format(SR.XmlSerializerUnsupportedType, type.FullName));
            }

            // check to see if the type has public default constructor for classes
            if (kind == TypeKind.Class && !type.IsAbstract)
            {
                flags |= GetConstructorFlags(type);
            }
            // check if a struct-like type is enumerable
            if (kind == TypeKind.Struct || kind == TypeKind.Class)
            {
                if (typeof(IEnumerable).IsAssignableFrom(type) && !IsArraySegment(type))
                {
                    arrayElementType = GetEnumeratorElementType(type, ref flags);
                    kind = TypeKind.Enumerable;

                    // GetEnumeratorElementType checks for the security attributes on the GetEnumerator(), Add() methods and Current property,
                    // we need to check the MoveNext() and ctor methods for the security attribues
                    flags |= GetConstructorFlags(type);
                }
            }
            typeDesc = new TypeDesc(type, CodeIdentifier.MakeValid(TypeName(type)), type.ToString(), kind, null, flags, null);
            typeDesc.Exception = exception;

            if (directReference && (typeDesc.IsClass || kind == TypeKind.Serializable))
                typeDesc.CheckNeedConstructor();

            if (typeDesc.IsUnsupported)
            {
                // return right away, do not check anything else
                return typeDesc;
            }
            _typeDescs.Add(type, typeDesc);

            if (arrayElementType != null)
            {
                TypeDesc td = GetTypeDesc(arrayElementType, memberInfo, true, false);
                // explicitly disallow read-only elements, even if they are collections
                if (directReference && (td.IsCollection || td.IsEnumerable) && !td.IsPrimitive)
                {
                    td.CheckNeedConstructor();
                }
                typeDesc.ArrayElementTypeDesc = td;
            }
            if (baseType != null && baseType != typeof(object) && baseType != typeof(ValueType))
            {
                typeDesc.BaseTypeDesc = GetTypeDesc(baseType, memberInfo, false, false);
            }
            if (type.IsNestedPublic)
            {
                for (Type? t = type.DeclaringType; t != null && !t.ContainsGenericParameters && !(t.IsAbstract && t.IsSealed); t = t.DeclaringType)
                    GetTypeDesc(t, null, false);
            }
            return typeDesc;
        }

        private static bool IsArraySegment(Type t)
        {
            return t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(ArraySegment<>));
        }

        internal static bool IsOptionalValue(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
                    return true;
            }
            return false;
        }

        /*
        static string GetHash(string str) {
            MD5 md5 = MD5.Create();
            string hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(str)), 0, 6).Replace("+", "_P").Replace("/", "_S");
            return hash;
        }
        */

        internal static string TypeName(Type t)
        {
            if (t.IsArray)
            {
                return $"ArrayOf{TypeName(t.GetElementType()!)}";
            }
            else if (t.IsGenericType)
            {
                StringBuilder typeName = new StringBuilder();
                StringBuilder ns = new StringBuilder();
                string name = t.Name;
                int arity = name.IndexOf('`');
                if (arity >= 0)
                {
                    name = name.Substring(0, arity);
                }
                typeName.Append(name);
                typeName.Append("Of");
                Type[] arguments = t.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    typeName.Append(TypeName(arguments[i]));
                    ns.Append(arguments[i].Namespace);
                }
                /*
                if (ns.Length > 0) {
                    typeName.Append('_');
                    typeName.Append(GetHash(ns.ToString()));
                }
                */
                return typeName.ToString();
            }
            return t.Name;
        }

        [RequiresUnreferencedCode("calls GetEnumeratorElementType")]
        internal static Type? GetArrayElementType(Type type, string? memberInfo)
        {
            if (type.IsArray)
                return type.GetElementType();
            else if (IsArraySegment(type))
                return null;
            else if (typeof(ICollection).IsAssignableFrom(type))
                return GetCollectionElementType(type, memberInfo);
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                TypeFlags flags = TypeFlags.None;
                return GetEnumeratorElementType(type, ref flags);
            }
            else
                return null;
        }

        internal static MemberMapping[] GetAllMembers(StructMapping mapping)
        {
            if (mapping.BaseMapping == null)
                return mapping.Members!;
            var list = new List<MemberMapping>();
            GetAllMembers(mapping, list);
            return list.ToArray();
        }

        internal static void GetAllMembers(StructMapping mapping, List<MemberMapping> list)
        {
            if (mapping.BaseMapping != null)
            {
                GetAllMembers(mapping.BaseMapping, list);
            }
            for (int i = 0; i < mapping.Members!.Length; i++)
            {
                list.Add(mapping.Members[i]);
            }
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        internal static MemberMapping[] GetAllMembers(StructMapping mapping, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos)
        {
            MemberMapping[] mappings = GetAllMembers(mapping);
            PopulateMemberInfos(mapping, mappings, memberInfos);
            return mappings;
        }

        internal static MemberMapping[] GetSettableMembers(StructMapping structMapping)
        {
            var list = new List<MemberMapping>();
            GetSettableMembers(structMapping, list);
            return list.ToArray();
        }

        private static void GetSettableMembers(StructMapping mapping, List<MemberMapping> list)
        {
            if (mapping.BaseMapping != null)
            {
                GetSettableMembers(mapping.BaseMapping, list);
            }

            if (mapping.Members != null)
            {
                foreach (MemberMapping memberMapping in mapping.Members)
                {
                    MemberInfo? memberInfo = memberMapping.MemberInfo;
                    PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
                    if (propertyInfo != null && !CanWriteProperty(propertyInfo, memberMapping.TypeDesc!))
                    {
                        throw new InvalidOperationException(SR.Format(SR.XmlReadOnlyPropertyError, propertyInfo.DeclaringType, propertyInfo.Name));
                    }
                    list.Add(memberMapping);
                }
            }
        }

        private static bool CanWriteProperty(PropertyInfo propertyInfo, TypeDesc typeDesc)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(typeDesc != null);

            // If the property is a collection, we don't need a setter.
            if (typeDesc.Kind == TypeKind.Collection || typeDesc.Kind == TypeKind.Enumerable)
            {
                return true;
            }
            // Else the property needs a public setter.
            return propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic;
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        internal static MemberMapping[] GetSettableMembers(StructMapping mapping, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos)
        {
            MemberMapping[] mappings = GetSettableMembers(mapping);
            PopulateMemberInfos(mapping, mappings, memberInfos);
            return mappings;
        }

        [RequiresUnreferencedCode("Calls ShouldBeReplaced with type whose members may be trimmed")]
        private static void PopulateMemberInfos(StructMapping structMapping, MemberMapping[] mappings, System.Collections.Generic.Dictionary<string, MemberInfo> memberInfos)
        {
            memberInfos.Clear();
            for (int i = 0; i < mappings.Length; ++i)
            {
                memberInfos[mappings[i].Name] = mappings[i].MemberInfo!;
                if (mappings[i].ChoiceIdentifier != null)
                    memberInfos[mappings[i].ChoiceIdentifier!.MemberName!] = mappings[i].ChoiceIdentifier!.MemberInfo!;
                if (mappings[i].CheckSpecifiedMemberInfo != null)
                    memberInfos[$"{mappings[i].Name}Specified"] = mappings[i].CheckSpecifiedMemberInfo!;
            }

            // The scenario here is that user has one base class A and one derived class B and wants to serialize/deserialize an object of B.
            // There's one virtual property defined in A and overridden by B. Without the replacing logic below, the code generated will always
            // try to access the property defined in A, rather than B.
            // The logic here is to:
            // 1) Check current members inside memberInfos dictionary and figure out whether there's any override or new properties defined in the derived class.
            //    If so, replace the one on the base class with the one on the derived class.
            // 2) Do the same thing for the memberMapping array. Note that we need to create a new copy of MemberMapping object since the old one could still be referenced
            //    by the StructMapping of the baseclass, so updating it directly could lead to other issues.
            Dictionary<string, MemberInfo>? replaceList = null;
            MemberInfo? replacedInfo;
            foreach (KeyValuePair<string, MemberInfo> pair in memberInfos)
            {
                if (ShouldBeReplaced(pair.Value, structMapping.TypeDesc!.Type!, out replacedInfo))
                {
                    replaceList ??= new Dictionary<string, MemberInfo>();

                    replaceList.Add(pair.Key, replacedInfo);
                }
            }

            if (replaceList != null)
            {
                foreach (KeyValuePair<string, MemberInfo> pair in replaceList)
                {
                    memberInfos[pair.Key] = pair.Value;
                }
                for (int i = 0; i < mappings.Length; i++)
                {
                    if (replaceList.TryGetValue(mappings[i].Name, out MemberInfo? mi))
                    {
                        MemberMapping newMapping = mappings[i].Clone();
                        newMapping.MemberInfo = mi;
                        mappings[i] = newMapping;
                    }
                }
            }
        }

        // The DynamicallyAccessedMemberTypes.All annotation is required here because the method
        // tries to access private members on base types (which is normally blocked by reflection)
        // This doesn't make the requirements worse since the only callers already have the type
        // annotated as All anyway.
        private static bool ShouldBeReplaced(
            MemberInfo memberInfoToBeReplaced,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type derivedType,
            out MemberInfo replacedInfo)
        {
            replacedInfo = memberInfoToBeReplaced;
            Type currentType = derivedType;
            Type typeToBeReplaced = memberInfoToBeReplaced.DeclaringType!;

            if (typeToBeReplaced.IsAssignableFrom(currentType))
            {
                while (currentType != typeToBeReplaced)
                {
                    const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

                    foreach (PropertyInfo info in currentType.GetProperties(DeclaredOnlyLookup))
                    {
                        if (info.Name == memberInfoToBeReplaced.Name)
                        {
                            // we have a new modifier situation: property names are the same but the declaring types are different
                            replacedInfo = info;
                            if (replacedInfo != memberInfoToBeReplaced)
                            {
                                // The property name is a match. It might be an override, or
                                // it might be hiding. Either way, check to see if the derived
                                // property has a getter that is usable for serialization.
                                if (info.GetMethod != null && !info.GetMethod!.IsPublic
                                    && memberInfoToBeReplaced is PropertyInfo
                                    && ((PropertyInfo)memberInfoToBeReplaced).GetMethod!.IsPublic
                                   )
                                {
                                    break;
                                }

                                return true;
                            }
                        }
                    }

                    foreach (FieldInfo info in currentType.GetFields(DeclaredOnlyLookup))
                    {
                        if (info.Name == memberInfoToBeReplaced.Name)
                        {
                            // we have a new modifier situation: field names are the same but the declaring types are different
                            replacedInfo = info;
                            if (replacedInfo != memberInfoToBeReplaced)
                            {
                                return true;
                            }
                        }
                    }

                    // we go one level down and try again
                    currentType = currentType.BaseType!;
                }
            }

            return false;
        }

        private static TypeFlags GetConstructorFlags(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors
                | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
        {
            ConstructorInfo? ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
            if (ctor != null)
            {
                TypeFlags flags = TypeFlags.HasDefaultConstructor;
                if (!ctor.IsPublic)
                    flags |= TypeFlags.CtorInaccessible;
                else
                {
                    object[] attrs = ctor.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        ObsoleteAttribute obsolete = (ObsoleteAttribute)attrs[0];
                        if (obsolete.IsError)
                        {
                            flags |= TypeFlags.CtorInaccessible;
                        }
                    }
                }
                return flags;
            }
            return 0;
        }

        [RequiresUnreferencedCode("Needs to mark members on the return type of the GetEnumerator method")]
        private static Type? GetEnumeratorElementType(Type type, ref TypeFlags flags)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                MethodInfo? enumerator = type.GetMethod("GetEnumerator", Type.EmptyTypes);

                if (enumerator == null || !typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType))
                {
                    // try generic implementation
                    enumerator = null;
                    foreach (MemberInfo member in type.GetMember("System.Collections.Generic.IEnumerable<*", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        enumerator = member as MethodInfo;
                        if (enumerator != null && typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType))
                        {
                            // use the first one we find
                            flags |= TypeFlags.GenericInterface;
                            break;
                        }
                        else
                        {
                            enumerator = null;
                        }
                    }
                    if (enumerator == null)
                    {
                        // and finally private interface implementation
                        flags |= TypeFlags.UsePrivateImplementation;
                        enumerator = type.GetMethod("System.Collections.IEnumerable.GetEnumerator", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
                    }
                }
                if (enumerator == null || !typeof(IEnumerator).IsAssignableFrom(enumerator.ReturnType))
                {
                    return null;
                }
                XmlAttributes methodAttrs = new XmlAttributes(enumerator);
                if (methodAttrs.XmlIgnore) return null;

                PropertyInfo? p = enumerator.ReturnType.GetProperty("Current");
                Type currentType = (p == null ? typeof(object) : p.PropertyType);

                MethodInfo? addMethod = type.GetMethod("Add", new Type[] { currentType });

                if (addMethod == null && currentType != typeof(object))
                {
                    currentType = typeof(object);
                    addMethod = type.GetMethod("Add", new Type[] { currentType });
                }
                if (addMethod == null)
                {
                    throw new InvalidOperationException(SR.Format(SR.XmlNoAddMethod, type.FullName, currentType, "IEnumerable"));
                }
                return currentType;
            }
            else
            {
                return null;
            }
        }

        internal static PropertyInfo GetDefaultIndexer(
            [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type, string? memberInfo)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (memberInfo == null)
                {
                    throw new NotSupportedException(SR.Format(SR.XmlUnsupportedIDictionary, type.FullName));
                }
                else
                {
                    throw new NotSupportedException(SR.Format(SR.XmlUnsupportedIDictionaryDetails, memberInfo, type.FullName));
                }
            }

            MemberInfo[] defaultMembers = type.GetDefaultMembers();
            PropertyInfo? indexer = null;
            if (defaultMembers != null && defaultMembers.Length > 0)
            {
                for (Type? t = type; t != null; t = t.BaseType)
                {
                    for (int i = 0; i < defaultMembers.Length; i++)
                    {
                        if (defaultMembers[i] is PropertyInfo defaultProp)
                        {
                            if (defaultProp.DeclaringType != t) continue;
                            if (!defaultProp.CanRead) continue;
                            MethodInfo getMethod = defaultProp.GetMethod!;
                            ParameterInfo[] parameters = getMethod.GetParameters();
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                            {
                                indexer = defaultProp;
                                break;
                            }
                        }
                    }
                    if (indexer != null) break;
                }
            }
            if (indexer == null)
            {
                throw new InvalidOperationException(SR.Format(SR.XmlNoDefaultAccessors, type.FullName));
            }
            MethodInfo? addMethod = type.GetMethod("Add", new Type[] { indexer.PropertyType });
            if (addMethod == null)
            {
                throw new InvalidOperationException(SR.Format(SR.XmlNoAddMethod, type.FullName, indexer.PropertyType, "ICollection"));
            }
            return indexer;
        }
        private static Type GetCollectionElementType(
            [DynamicallyAccessedMembers(TrimmerConstants.PublicMembers)] Type type,
            string? memberInfo)
        {
            return GetDefaultIndexer(type, memberInfo).PropertyType;
        }

        internal static XmlQualifiedName ParseWsdlArrayType(string type, out string dims, XmlSchemaObject? parent)
        {
            string ns;
            string name;

            int nsLen = type.LastIndexOf(':');

            if (nsLen <= 0)
            {
                ns = "";
            }
            else
            {
                ns = type.Substring(0, nsLen);
            }
            int nameLen = type.IndexOf('[', nsLen + 1);

            if (nameLen <= nsLen)
            {
                throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayTypeSyntax, type));
            }
            name = type.Substring(nsLen + 1, nameLen - nsLen - 1);
            dims = type.Substring(nameLen);

            // parent is not null only in the case when we used XmlSchema.Read(),
            // in which case we need to fixup the wsdl:arayType attribute value
            while (parent != null)
            {
                if (parent.Namespaces != null)
                {
                    if (parent.Namespaces.TryLookupNamespace(ns, out string? wsdlNs) && wsdlNs != null)
                    {
                        ns = wsdlNs;
                        break;
                    }
                }
                parent = parent.Parent;
            }

            return new XmlQualifiedName(name, ns);
        }

        internal ICollection Types
        {
            get { return _typeDescs.Keys; }
        }

        internal void AddTypeMapping(TypeMapping typeMapping)
        {
            _typeMappings.Add(typeMapping);
        }

        internal ICollection TypeMappings
        {
            get { return _typeMappings; }
        }
        internal static Dictionary<Type, TypeDesc> PrimitiveTypes { get { return s_primitiveTypes; } }
    }
}
