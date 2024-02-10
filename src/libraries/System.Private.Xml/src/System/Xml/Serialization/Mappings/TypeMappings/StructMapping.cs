// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization.Mappings.AccessorMappings;
using System.Xml.Serialization.Mappings.Accessors;
using System.Xml.Serialization.Mappings.Navigation;

namespace System.Xml.Serialization.Mappings.TypeMappings
{
    internal sealed class StructMapping : TypeMapping, INavigationNameScope<ElementAccessor>
    {
        private StructMapping? _baseMapping;
        private StructMapping? _derivedMappings;
        private StructMapping? _nextDerivedMapping;
        private MemberMapping? _xmlnsMember;
        private bool _hasSimpleContent;
        private bool _openModel;
        private bool _isSequence;
        private CodeIdentifiers? _scope;

        [DisallowNull]
        internal StructMapping? BaseMapping
        {
            get { return _baseMapping; }
            set
            {
                _baseMapping = value;
                if (!IsAnonymousType && _baseMapping != null)
                {
                    _nextDerivedMapping = _baseMapping._derivedMappings;
                    _baseMapping._derivedMappings = this;
                }
                if (value._isSequence && !_isSequence)
                {
                    _isSequence = true;
                    if (_baseMapping!.IsSequence)
                    {
                        for (StructMapping? derived = _derivedMappings; derived != null; derived = derived.NextDerivedMapping)
                        {
                            derived.SetSequence();
                        }
                    }
                }
            }
        }

        internal StructMapping? DerivedMappings
        {
            get { return _derivedMappings; }
        }

        internal bool IsFullyInitialized
        {
            get { return _baseMapping != null && Members != null; }
        }

        internal NavigationNameTable<ElementAccessor> LocalElements { get; } = new NavigationNameTable<ElementAccessor>();
        internal NavigationNameTable<AttributeAccessor> LocalAttributes { get; } = new NavigationNameTable<AttributeAccessor>();
        ElementAccessor? INavigationNameScope<ElementAccessor>.this[string? name, string? ns]
        {
            get
            {
                ElementAccessor? named = LocalElements[name, ns];
                if (named != null)
                {
                    return named;
                }
                if (_baseMapping != null)
                {
                    return ((INavigationNameScope<ElementAccessor>)_baseMapping)[name, ns];
                }
                return null;
            }
            set
            {
                LocalElements[name, ns] = value;
            }
        }
        internal StructMapping? NextDerivedMapping
        {
            get { return _nextDerivedMapping; }
        }

        internal bool HasSimpleContent
        {
            get { return _hasSimpleContent; }
        }

        internal bool HasXmlnsMember
        {
            get
            {
                StructMapping? mapping = this;
                while (mapping != null)
                {
                    if (mapping.XmlnsMember != null)
                        return true;
                    mapping = mapping.BaseMapping;
                }
                return false;
            }
        }

        internal MemberMapping[]? Members { get; set; }

        internal MemberMapping? XmlnsMember
        {
            get { return _xmlnsMember; }
            set { _xmlnsMember = value; }
        }

        internal bool IsOpenModel
        {
            get { return _openModel; }
            set { _openModel = value; }
        }

        internal CodeIdentifiers Scope
        {
            get => _scope ??= new CodeIdentifiers();
            set => _scope = value;
        }

        internal MemberMapping? FindDeclaringMapping(MemberMapping member, out StructMapping? declaringMapping, string? parent)
        {
            declaringMapping = null;
            if (BaseMapping != null)
            {
                MemberMapping? baseMember = BaseMapping.FindDeclaringMapping(member, out declaringMapping, parent);
                if (baseMember != null)
                {
                    return baseMember;
                }
            }
            if (Members == null)
            {
                return null;
            }

            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i].Name == member.Name)
                {
                    if (Members[i].TypeDesc != member.TypeDesc)
                    {
                        throw new InvalidOperationException(SR.Format(SR.XmlHiddenMember, parent, member.Name, member.TypeDesc!.FullName, this.TypeName, Members[i].Name, Members[i].TypeDesc!.FullName));
                    }
                    else if (!Members[i].Match(member))
                    {
                        throw new InvalidOperationException(SR.Format(SR.XmlInvalidXmlOverride, parent, member.Name, this.TypeName, Members[i].Name));
                    }

                    declaringMapping = this;
                    return Members[i];
                }
            }
            return null;
        }
        internal bool Declares(MemberMapping member, string? parent)
        {
            return (FindDeclaringMapping(member, out _, parent) != null);
        }

        internal void SetContentModel(TextAccessor? text, bool hasElements)
        {
            if (BaseMapping == null || BaseMapping.TypeDesc!.IsRoot)
            {
                _hasSimpleContent = !hasElements && text != null && !text.Mapping!.IsList;
            }
            else if (BaseMapping.HasSimpleContent)
            {
                if (text != null || hasElements)
                {
                    // we can only extent a simleContent type with attributes
                    throw new InvalidOperationException(SR.Format(SR.XmlIllegalSimpleContentExtension, TypeDesc!.FullName, BaseMapping.TypeDesc.FullName));
                }
                else
                {
                    _hasSimpleContent = true;
                }
            }
            else
            {
                _hasSimpleContent = false;
            }
            if (!_hasSimpleContent && text != null && !text.Mapping!.TypeDesc!.CanBeTextValue && !(BaseMapping != null && !BaseMapping.TypeDesc!.IsRoot && (text.Mapping.TypeDesc.IsEnum || text.Mapping.TypeDesc.IsPrimitive)))
            {
                throw new InvalidOperationException(SR.Format(SR.XmlIllegalTypedTextAttribute, TypeDesc!.FullName, text.Name, text.Mapping.TypeDesc.FullName));
            }
        }

        internal bool HasExplicitSequence()
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    if (Members[i].IsParticle && Members[i].IsSequence)
                    {
                        return true;
                    }
                }
            }
            return (_baseMapping != null && _baseMapping.HasExplicitSequence());
        }

        internal void SetSequence()
        {
            if (TypeDesc!.IsRoot)
            {
                return;
            }

            StructMapping start = this;

            // find first mapping that does not have the sequence set
            while (start.BaseMapping != null && !start.BaseMapping.IsSequence && !start.BaseMapping.TypeDesc!.IsRoot)
            {
                start = start.BaseMapping;
            }

            start.IsSequence = true;
            for (StructMapping? derived = start.DerivedMappings; derived != null; derived = derived.NextDerivedMapping)
            {
                derived.SetSequence();
            }
        }

        internal bool IsSequence
        {
            get { return _isSequence && !TypeDesc!.IsRoot; }
            set { _isSequence = value; }
        }
    }
}
