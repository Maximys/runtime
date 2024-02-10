// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Serialization.Mappings.Accessors;
using System.Xml.Serialization.Mappings.Accessors.Comparers;
using System.Xml.Serialization.Types;

namespace System.Xml.Serialization.Mappings.AccessorMappings
{
    internal abstract class AccessorMapping : Mapping
    {
        private ElementAccessor[]? _elements;
        private ElementAccessor[]? _sortedElements;

        internal AccessorMapping()
        { }

        protected AccessorMapping(AccessorMapping mapping)
            : base(mapping)
        {
            TypeDesc = mapping.TypeDesc;
            Attribute = mapping.Attribute;
            _elements = mapping._elements;
            _sortedElements = mapping._sortedElements;
            Text = mapping.Text;
            ChoiceIdentifier = mapping.ChoiceIdentifier;
            Xmlns = mapping.Xmlns;
            Ignore = mapping.Ignore;
        }

        internal bool IsAttribute
        {
            get { return Attribute != null; }
        }

        internal bool IsText
        {
            get { return Text != null && (_elements == null || _elements.Length == 0); }
        }

        internal bool IsParticle
        {
            get { return (_elements != null && _elements.Length > 0); }
        }

        internal TypeDesc? TypeDesc { get; set; }

        internal AttributeAccessor? Attribute { get; set; }

        internal ElementAccessor[]? Elements
        {
            get { return _elements; }
            set { _elements = value; _sortedElements = null; }
        }

        internal static void SortMostToLeastDerived(ElementAccessor[] elements)
        {
            Array.Sort(elements, new AccessorComparer());
        }

        internal ElementAccessor[]? ElementsSortedByDerivation
        {
            get
            {
                if (_sortedElements != null)
                {
                    return _sortedElements;
                }

                if (_elements == null)
                {
                    return null;
                }

                _sortedElements = new ElementAccessor[_elements.Length];
                Array.Copy(_elements, _sortedElements, _elements.Length);
                SortMostToLeastDerived(_sortedElements);
                return _sortedElements;
            }
        }

        internal TextAccessor? Text { get; set; }

        internal ChoiceIdentifierAccessor? ChoiceIdentifier { get; set; }

        internal XmlnsAccessor? Xmlns { get; set; }

        internal bool Ignore { get; set; }

        internal Accessor? Accessor
        {
            get
            {
                if (Xmlns != null)
                {
                    return Xmlns;
                }

                if (Attribute != null)
                {
                    return Attribute;
                }

                if (_elements != null && _elements.Length > 0)
                {
                    return _elements[0];
                }

                return Text;
            }
        }

        internal static bool ElementsMatch(ElementAccessor[]? a, ElementAccessor[]? b)
        {
            if (a == null)
            {
                if (b == null)
                {
                    return true;
                }

                return false;
            }
            if (b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Name != b[i].Name || a[i].Namespace != b[i].Namespace || a[i].Form != b[i].Form || a[i].IsNullable != b[i].IsNullable)
                {
                    return false;
                }
            }

            return true;
        }

        internal bool Match(AccessorMapping mapping)
        {
            if (Elements != null && Elements.Length > 0)
            {
                if (!ElementsMatch(Elements, mapping.Elements))
                {
                    return false;
                }

                if (Text == null)
                {
                    return (mapping.Text == null);
                }
            }

            if (Attribute != null)
            {
                if (mapping.Attribute == null)
                {
                    return false;
                }

                return (Attribute.Name == mapping.Attribute.Name && Attribute.Namespace == mapping.Attribute.Namespace && Attribute.Form == mapping.Attribute.Form);
            }

            if (Text != null)
            {
                return (mapping.Text != null);
            }

            return (mapping.Accessor == null);
        }
    }
}
