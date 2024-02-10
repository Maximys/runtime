// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;

namespace System.Xml.Serialization.Types
{
    internal sealed class TypeDesc
    {
        private readonly string _name;
        private readonly string _fullName;
        private string? _cSharpName;
        private TypeDesc? _arrayElementTypeDesc;
        private TypeDesc? _arrayTypeDesc;
        private TypeDesc? _nullableTypeDesc;
        private readonly TypeKind _kind;
        private readonly XmlSchemaType? _dataType;
        private Type? _type;
        private TypeDesc? _baseTypeDesc;
        private TypeFlags _flags;
        private readonly string? _formatterName;
        private readonly bool _isXsdType;
        private bool _isMixed;
        private int _weight;
        private Exception? _exception;

        internal TypeDesc(string name, string fullName, XmlSchemaType? dataType, TypeKind kind, TypeDesc? baseTypeDesc, TypeFlags flags, string? formatterName)
        {
            _name = name.Replace('+', '.');
            _fullName = fullName.Replace('+', '.');
            _kind = kind;
            _baseTypeDesc = baseTypeDesc;
            _flags = flags;
            _isXsdType = kind == TypeKind.Primitive;
            if (_isXsdType)
                _weight = 1;
            else if (kind == TypeKind.Enum)
                _weight = 2;
            else if (_kind == TypeKind.Root)
                _weight = -1;
            else
                _weight = baseTypeDesc == null ? 0 : baseTypeDesc.Weight + 1;
            _dataType = dataType;
            _formatterName = formatterName;
        }

        internal TypeDesc(string name, string fullName, TypeKind kind, TypeDesc? baseTypeDesc, TypeFlags flags)
            : this(name, fullName, (XmlSchemaType?)null, kind, baseTypeDesc, flags, null)
        { }

        internal TypeDesc(
            Type type, bool isXsdType, XmlSchemaType dataType, string formatterName, TypeFlags flags)
            : this(type!.Name, type.FullName!, dataType, TypeKind.Primitive, (TypeDesc?)null, flags, formatterName)
        {
            _isXsdType = isXsdType;
            _type = type;
        }
        internal TypeDesc(
            Type? type, string name, string fullName, TypeKind kind, TypeDesc? baseTypeDesc, TypeFlags flags, TypeDesc? arrayElementTypeDesc)
            : this(name, fullName, null, kind, baseTypeDesc, flags, null)
        {
            _arrayElementTypeDesc = arrayElementTypeDesc;
            _type = type;
        }

        public override string ToString()
        {
            return _fullName;
        }

        internal TypeFlags Flags
        {
            get { return _flags; }
        }

        internal bool IsXsdType
        {
            get { return _isXsdType; }
        }

        internal static bool IsMappedType
        {
            get { return false; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal string FullName
        {
            get { return _fullName; }
        }

        internal string CSharpName =>
            _cSharpName ??= _type == null ? CodeIdentifier.GetCSharpName(_fullName) : CodeIdentifier.GetCSharpName(_type);

        internal XmlSchemaType? DataType
        {
            get { return _dataType; }
        }

        internal Type? Type
        {
            get { return _type; }
        }

        internal string? FormatterName
        {
            get { return _formatterName; }
        }

        internal TypeKind Kind
        {
            get { return _kind; }
        }

        internal bool IsValueType
        {
            get { return (_flags & TypeFlags.Reference) == 0; }
        }

        internal bool CanBeAttributeValue
        {
            get { return (_flags & TypeFlags.CanBeAttributeValue) != 0; }
        }

        internal bool XmlEncodingNotRequired
        {
            get { return (_flags & TypeFlags.XmlEncodingNotRequired) != 0; }
        }

        internal bool CanBeElementValue
        {
            get { return (_flags & TypeFlags.CanBeElementValue) != 0; }
        }

        internal bool CanBeTextValue
        {
            get { return (_flags & TypeFlags.CanBeTextValue) != 0; }
        }

        internal bool IsMixed
        {
            get { return _isMixed || CanBeTextValue; }
            set { _isMixed = value; }
        }

        internal bool IsSpecial
        {
            get { return (_flags & TypeFlags.Special) != 0; }
        }

        internal bool IsAmbiguousDataType
        {
            get { return (_flags & TypeFlags.AmbiguousDataType) != 0; }
        }

        internal bool HasCustomFormatter
        {
            get { return (_flags & TypeFlags.HasCustomFormatter) != 0; }
        }

        internal bool HasDefaultSupport
        {
            get { return (_flags & TypeFlags.IgnoreDefault) == 0; }
        }

        internal bool HasIsEmpty
        {
            get { return (_flags & TypeFlags.HasIsEmpty) != 0; }
        }

        internal bool CollapseWhitespace
        {
            get { return (_flags & TypeFlags.CollapseWhitespace) != 0; }
        }

        internal bool HasDefaultConstructor
        {
            get { return (_flags & TypeFlags.HasDefaultConstructor) != 0; }
        }

        internal bool IsUnsupported
        {
            get { return (_flags & TypeFlags.Unsupported) != 0; }
        }

        internal bool IsGenericInterface
        {
            get { return (_flags & TypeFlags.GenericInterface) != 0; }
        }

        internal bool IsPrivateImplementation
        {
            get { return (_flags & TypeFlags.UsePrivateImplementation) != 0; }
        }

        internal bool CannotNew
        {
            get { return !HasDefaultConstructor || ConstructorInaccessible; }
        }

        internal bool IsAbstract
        {
            get { return (_flags & TypeFlags.Abstract) != 0; }
        }

        internal bool IsOptionalValue
        {
            get { return (_flags & TypeFlags.OptionalValue) != 0; }
        }

        internal bool UseReflection
        {
            get { return (_flags & TypeFlags.UseReflection) != 0; }
        }

        internal bool IsVoid
        {
            get { return _kind == TypeKind.Void; }
        }

        internal bool IsClass
        {
            get { return _kind == TypeKind.Class; }
        }

        internal bool IsStructLike
        {
            get { return _kind == TypeKind.Struct || _kind == TypeKind.Class; }
        }

        internal bool IsArrayLike
        {
            get { return _kind == TypeKind.Array || _kind == TypeKind.Collection || _kind == TypeKind.Enumerable; }
        }

        internal bool IsCollection
        {
            get { return _kind == TypeKind.Collection; }
        }

        internal bool IsEnumerable
        {
            get { return _kind == TypeKind.Enumerable; }
        }

        internal bool IsArray
        {
            get { return _kind == TypeKind.Array; }
        }

        internal bool IsPrimitive
        {
            get { return _kind == TypeKind.Primitive; }
        }

        internal bool IsEnum
        {
            get { return _kind == TypeKind.Enum; }
        }

        internal bool IsNullable
        {
            get { return !IsValueType; }
        }

        internal bool IsRoot
        {
            get { return _kind == TypeKind.Root; }
        }

        internal bool ConstructorInaccessible
        {
            get { return (_flags & TypeFlags.CtorInaccessible) != 0; }
        }

        internal Exception? Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        internal TypeDesc GetNullableTypeDesc(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        {
            if (IsOptionalValue)
                return this;

            if (_nullableTypeDesc == null)
            {
                _nullableTypeDesc = new TypeDesc($"NullableOf{_name}", $"System.Nullable`1[{_fullName}]", null, TypeKind.Struct, this, _flags | TypeFlags.OptionalValue, _formatterName);
                _nullableTypeDesc._type = type;
            }

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
            _arrayElementTypeDesc?.CheckSupported();
        }

        internal void CheckNeedConstructor()
        {
            if (!IsValueType && !IsAbstract && !HasDefaultConstructor)
            {
                _flags |= TypeFlags.Unsupported;
                _exception = new InvalidOperationException(SR.Format(SR.XmlConstructorInaccessible, FullName));
            }
        }

        internal TypeDesc? ArrayElementTypeDesc
        {
            get { return _arrayElementTypeDesc; }
            set { _arrayElementTypeDesc = value; }
        }

        internal int Weight
        {
            get { return _weight; }
        }

        internal TypeDesc CreateArrayTypeDesc() => _arrayTypeDesc ??= new TypeDesc(null, $"{_name}[]", $"{_fullName}[]", TypeKind.Array, null, TypeFlags.Reference | (_flags & TypeFlags.UseReflection), this);

        internal TypeDesc? BaseTypeDesc
        {
            get { return _baseTypeDesc; }
            set
            {
                _baseTypeDesc = value;
                _weight = _baseTypeDesc == null ? 0 : _baseTypeDesc.Weight + 1;
            }
        }

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
