// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;

namespace System.Xml.Serialization.Mappings.Accessors
{
    internal abstract class Accessor
    {
        private string? _name;

        internal Accessor() { }

        internal TypeMapping? Mapping { get; set; }

        internal object? Default { get; set; }

        internal bool HasDefault
        {
            get { return Default != null && Default != DBNull.Value; }
        }

        [AllowNull]
        internal virtual string Name
        {
            get { return _name ?? string.Empty; }
            set { _name = value; }
        }

        internal bool Any{ get; set; }

        internal string? AnyNamespaces { get; set; }

        internal string? Namespace { get; set; }

        internal XmlSchemaForm Form { get; set; } = XmlSchemaForm.None;

        internal bool IsFixed { get; set; }

        internal bool IsOptional { get; set; }

        internal bool IsTopLevelInSchema { get; set; }

        [return: NotNullIfNotNull(nameof(name))]
        internal static string? EscapeName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return XmlConvert.EncodeLocalName(name);
        }

        [return: NotNullIfNotNull(nameof(name))]
        internal static string? EscapeQName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            int colon = name.LastIndexOf(':');
            if (colon < 0)
            {
                return XmlConvert.EncodeLocalName(name);
            }
            else
            {
                if (colon == 0 || colon == name.Length - 1)
                {
                    throw new ArgumentException(SR.Format(SR.Xml_InvalidNameChars, name), nameof(name));
                }

                return new XmlQualifiedName(XmlConvert.EncodeLocalName(name.Substring(colon + 1)), XmlConvert.EncodeLocalName(name.Substring(0, colon))).ToString();
            }
        }

        [return: NotNullIfNotNull(nameof(name))]
        internal static string? UnescapeName(string? name)
        {
            return XmlConvert.DecodeName(name);
        }

        internal string ToString(string? defaultNs)
        {
            if (Any)
            {
                return $"{Namespace ?? "##any"}:{Name}";
            }
            else
            {
                return Namespace == defaultNs ? Name : $"{Namespace}:{Name}";
            }
        }
    }
}
