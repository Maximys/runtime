// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings
{
    internal abstract class Mapping
    {
        internal Mapping() { }

        protected Mapping(Mapping mapping)
        {
            IsSoap = mapping.IsSoap;
        }

        internal bool IsSoap { get; set; }
    }
}
