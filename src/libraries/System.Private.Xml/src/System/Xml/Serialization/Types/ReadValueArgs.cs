// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Types
{
    internal struct ReadValueArgs
    {
        internal string Value { get; }

        internal bool Decode { get; }

        internal XmlReader Reader { get; }

        public ReadValueArgs(string value, bool decode, XmlReader reader)
        {
            Value = value;
            Decode = decode;
            Reader = reader;
        }
    }
}
