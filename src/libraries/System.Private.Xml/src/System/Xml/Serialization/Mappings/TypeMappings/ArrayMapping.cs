// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Serialization.Mappings.Accessors;

namespace System.Xml.Serialization.Mappings.TypeMappings
{
    internal sealed class ArrayMapping : TypeMapping
    {
        private ElementAccessor[]? _elements;
        private ElementAccessor[]? _sortedElements;

        internal ElementAccessor[]? Elements
        {
            get { return _elements; }
            set { _elements = value; _sortedElements = null; }
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
                AccessorMapping.SortMostToLeastDerived(_sortedElements);
                return _sortedElements;
            }
        }


        internal ArrayMapping? Next { get; set; }

        internal StructMapping? TopLevelMapping { get; set; }
    }
}
