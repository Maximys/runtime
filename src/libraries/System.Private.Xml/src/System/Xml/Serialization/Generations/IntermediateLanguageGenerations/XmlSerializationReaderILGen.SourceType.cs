// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Generations.IntermediateLanguageGenerations
{
    internal sealed partial class XmlSerializationReaderILGen
    {
        private enum SourceType
        {
            Array,
            Boolean,
            ReadElementString,
            ReadString,
            Value,
        }
    }
}
