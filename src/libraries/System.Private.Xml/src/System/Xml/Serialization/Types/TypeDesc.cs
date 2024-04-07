// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;

namespace System.Xml.Serialization.Types
{
    internal sealed class TypeDesc
    {
        private string? _cSharpName;
        private TypeDesc? _arrayTypeDesc;
        private TypeDesc? _nullableTypeDesc;

        private TypeDesc? _baseTypeDesc;
        private bool _isMixed;

        internal TypeDesc(
            string name,
            string fullName,
            XmlSchemaType? dataType,
            TypeKind kind,
            TypeDesc? baseTypeDesc,
            TypeFlags flags,
            Formatter? formatter)
        {
            Name = name.Replace('+', '.');
            FullName = fullName.Replace('+', '.');
            Kind = kind;
            _baseTypeDesc = baseTypeDesc;
            Flags = flags;
            IsXsdType = kind == TypeKind.Primitive;
            switch (Kind)
            {
                case TypeKind.Enum:
                    Weight = 2;
                    break;
                case TypeKind.Primitive:
                    Weight = 1;
                    break;
                case TypeKind.Root:
                    Weight = -1;
                    break;
                default:
                    Weight = baseTypeDesc == null ? 0 : baseTypeDesc.Weight + 1;
                    break;
            }
            DataType = dataType;
            Formatter = formatter;
        }

        internal TypeDesc(string name, string fullName, TypeKind kind, TypeDesc? baseTypeDesc, TypeFlags flags)
            : this(name, fullName, null, kind, baseTypeDesc, flags, null)
        { }

        internal TypeDesc(
            Type type,
            bool isXsdType,
            XmlSchemaType dataType,
            Formatter? formatter,
            TypeFlags flags)
            : this(type!.Name, type.FullName!, dataType, TypeKind.Primitive, null, flags, formatter)
        {
            IsXsdType = isXsdType;
            Type = type;
        }

        internal TypeDesc(
            Type? type, string name, string fullName, TypeKind kind, TypeDesc? baseTypeDesc, TypeFlags flags, TypeDesc? arrayElementTypeDesc)
            : this(name, fullName, null, kind, baseTypeDesc, flags, null)
        {
            ArrayElementTypeDesc = arrayElementTypeDesc;
            Type = type;
        }

        public override string ToString()
        {
            return FullName;
        }

        internal TypeDesc? ArrayElementTypeDesc { get; set; }

        internal TypeDesc? BaseTypeDesc
        {
            get { return _baseTypeDesc; }
            set
            {
                _baseTypeDesc = value;
                Weight = _baseTypeDesc == null ? 0 : _baseTypeDesc.Weight + 1;
            }
        }

        internal int Weight { get; private set; }

        internal TypeFlags Flags { get; private set; }

        internal bool IsXsdType { get; }

        internal static bool IsMappedType
        {
            get { return false; }
        }

        internal string Name { get; }

        internal string FullName { get; }

        internal string CSharpName =>
            _cSharpName ??= Type == null ? CodeIdentifier.GetCSharpName(FullName) : CodeIdentifier.GetCSharpName(Type);

        internal XmlSchemaType? DataType { get; }

        internal Type? Type { get; init; }

        internal Formatter? Formatter { get; }

        internal TypeKind Kind { get; }

        internal bool IsValueType
        {
            get { return (Flags & TypeFlags.Reference) == 0; }
        }

        internal bool CanBeAttributeValue
        {
            get { return (Flags & TypeFlags.CanBeAttributeValue) != 0; }
        }

        internal bool XmlEncodingNotRequired
        {
            get { return (Flags & TypeFlags.XmlEncodingNotRequired) != 0; }
        }

        internal bool CanBeElementValue
        {
            get { return (Flags & TypeFlags.CanBeElementValue) != 0; }
        }

        internal bool CanBeTextValue
        {
            get { return (Flags & TypeFlags.CanBeTextValue) != 0; }
        }

        internal bool IsMixed
        {
            get { return _isMixed || CanBeTextValue; }
            set { _isMixed = value; }
        }

        internal bool IsSpecial
        {
            get { return (Flags & TypeFlags.Special) != 0; }
        }

        internal bool IsAmbiguousDataType
        {
            get { return (Flags & TypeFlags.AmbiguousDataType) != 0; }
        }

        internal bool HasCustomFormatter
        {
            get { return (Flags & TypeFlags.HasCustomFormatter) != 0; }
        }

        internal bool HasDefaultSupport
        {
            get { return (Flags & TypeFlags.IgnoreDefault) == 0; }
        }

        internal bool HasIsEmpty
        {
            get { return (Flags & TypeFlags.HasIsEmpty) != 0; }
        }

        internal bool CollapseWhitespace
        {
            get { return (Flags & TypeFlags.CollapseWhitespace) != 0; }
        }

        internal bool HasDefaultConstructor
        {
            get { return (Flags & TypeFlags.HasDefaultConstructor) != 0; }
        }

        internal bool IsUnsupported
        {
            get { return (Flags & TypeFlags.Unsupported) != 0; }
        }

        internal bool IsGenericInterface
        {
            get { return (Flags & TypeFlags.GenericInterface) != 0; }
        }

        internal bool IsPrivateImplementation
        {
            get { return (Flags & TypeFlags.UsePrivateImplementation) != 0; }
        }

        internal bool CannotNew
        {
            get { return !HasDefaultConstructor || ConstructorInaccessible; }
        }

        internal bool IsAbstract
        {
            get { return (Flags & TypeFlags.Abstract) != 0; }
        }

        internal bool IsOptionalValue
        {
            get { return (Flags & TypeFlags.OptionalValue) != 0; }
        }

        internal bool UseReflection
        {
            get { return (Flags & TypeFlags.UseReflection) != 0; }
        }

        internal bool IsVoid
        {
            get { return Kind == TypeKind.Void; }
        }

        internal bool IsClass
        {
            get { return Kind == TypeKind.Class; }
        }

        internal bool IsStructLike
        {
            get { return Kind == TypeKind.Struct || Kind == TypeKind.Class; }
        }

        internal bool IsArrayLike
        {
            get { return Kind == TypeKind.Array || Kind == TypeKind.Collection || Kind == TypeKind.Enumerable; }
        }

        internal bool IsCollection
        {
            get { return Kind == TypeKind.Collection; }
        }

        internal bool IsEnumerable
        {
            get { return Kind == TypeKind.Enumerable; }
        }

        internal bool IsArray
        {
            get { return Kind == TypeKind.Array; }
        }

        internal bool IsPrimitive
        {
            get { return Kind == TypeKind.Primitive; }
        }

        internal bool IsEnum
        {
            get { return Kind == TypeKind.Enum; }
        }

        internal bool IsNullable
        {
            get { return !IsValueType; }
        }

        internal bool IsRoot
        {
            get { return Kind == TypeKind.Root; }
        }

        internal bool ConstructorInaccessible
        {
            get { return (Flags & TypeFlags.CtorInaccessible) != 0; }
        }

        internal Exception? Exception { get; set; }

        internal TypeDesc GetNullableTypeDesc(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        {
            if (IsOptionalValue)
            {
                return this;
            }

            _nullableTypeDesc ??= new TypeDesc($"NullableOf{Name}", $"System.Nullable`1[{FullName}]", null, TypeKind.Struct, this, Flags | TypeFlags.OptionalValue, Formatter)
            {
                Type = type
            };

            return _nullableTypeDesc;
        }
        internal void CheckSupported()
        {
            if (IsUnsupported)
            {
                if (Exception != null)
                {
                    throw Exception;
                }
                else
                {
                    throw new NotSupportedException(SR.Format(SR.XmlSerializerUnsupportedType, FullName));
                }
            }
            _baseTypeDesc?.CheckSupported();
            ArrayElementTypeDesc?.CheckSupported();
        }

        internal void CheckNeedConstructor()
        {
            if (!IsValueType && !IsAbstract && !HasDefaultConstructor)
            {
                Flags |= TypeFlags.Unsupported;
                Exception = new InvalidOperationException(SR.Format(SR.XmlConstructorInaccessible, FullName));
            }
        }

        internal TypeDesc CreateArrayTypeDesc() => _arrayTypeDesc ??= new TypeDesc(null, $"{Name}[]", $"{FullName}[]", TypeKind.Array, null, TypeFlags.Reference | (Flags & TypeFlags.UseReflection), this);

        internal bool IsDerivedFrom(TypeDesc baseTypeDesc)
        {
            TypeDesc? typeDesc = this;
            while (typeDesc != null)
            {
                if (typeDesc == baseTypeDesc) return true;
                typeDesc = typeDesc.BaseTypeDesc;
            }
            return baseTypeDesc.IsRoot;
        }

        internal static TypeDesc? FindCommonBaseTypeDesc(TypeDesc[] typeDescs)
        {
            if (typeDescs.Length == 0) return null;
            TypeDesc? leastDerivedTypeDesc = null;
            int leastDerivedLevel = int.MaxValue;

            for (int i = 0; i < typeDescs.Length; i++)
            {
                int derivationLevel = typeDescs[i].Weight;
                if (derivationLevel < leastDerivedLevel)
                {
                    leastDerivedLevel = derivationLevel;
                    leastDerivedTypeDesc = typeDescs[i];
                }
            }
            while (leastDerivedTypeDesc != null)
            {
                int i;
                for (i = 0; i < typeDescs.Length; i++)
                {
                    if (!typeDescs[i].IsDerivedFrom(leastDerivedTypeDesc)) break;
                }
                if (i == typeDescs.Length) break;
                leastDerivedTypeDesc = leastDerivedTypeDesc.BaseTypeDesc;
            }
            return leastDerivedTypeDesc;
        }
    }
}
