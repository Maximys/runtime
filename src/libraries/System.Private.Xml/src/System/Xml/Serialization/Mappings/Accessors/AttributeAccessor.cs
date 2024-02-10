// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Schema;

namespace System.Xml.Serialization.Mappings.Accessors
{
    internal sealed class AttributeAccessor : Accessor
    {
        private bool _isSpecial;
        private bool _isList;

        internal bool IsSpecialXmlNamespace
        {
            get { return _isSpecial; }
        }

        internal bool IsList
        {
            get { return _isList; }
            set { _isList = value; }
        }

        internal void CheckSpecial()
        {
            int colon = Name.LastIndexOf(':');

            if (colon >= 0)
            {
                if (!Name.StartsWith("xml:", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(SR.Format(SR.Xml_InvalidNameChars, Name));
                }
                Name = Name.Substring("xml:".Length);
                Namespace = XmlReservedNs.NsXml;
                _isSpecial = true;
            }
            else
            {
                if (Namespace == XmlReservedNs.NsXml)
                {
                    _isSpecial = true;
                }
                else
                {
                    _isSpecial = false;
                }
            }
            if (_isSpecial)
            {
                Form = XmlSchemaForm.Qualified;
            }
        }
    }
}
