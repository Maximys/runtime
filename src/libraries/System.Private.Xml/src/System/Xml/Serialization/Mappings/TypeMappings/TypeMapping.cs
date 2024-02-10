// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization.Types;

namespace System.Xml.Serialization.Mappings.TypeMappings
{
    internal abstract class TypeMapping : Mapping
    {
        private bool _referencedByElement;

        internal bool ReferencedByTopLevelElement { get; set; }

        internal bool ReferencedByElement
        {
            get { return _referencedByElement || ReferencedByTopLevelElement; }
            set { _referencedByElement = value; }
        }

        internal string? Namespace { get; set; }

        internal string? TypeName { get; set; }

        internal TypeDesc? TypeDesc { get; set; }

        internal bool IncludeInSchema { get; set; } = true;

        internal virtual bool IsList
        {
            get { return false; }
            set { }
        }

        internal bool IsReference { get; set; }

        [MemberNotNullWhen(false, nameof(TypeName))]
        internal bool IsAnonymousType
        {
            get { return string.IsNullOrEmpty(TypeName); }
        }

        internal virtual string DefaultElementName
        {
            get { return IsAnonymousType ? XmlConvert.EncodeLocalName(TypeDesc!.Name) : TypeName; }
        }
    }
}
