// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Serialization.Environments.Schemas;
using System.Xml.Serialization.Environments.Types;
using System.Xml.Serialization.Environments.Values;

namespace System.Xml.Serialization.Environments
{
    internal sealed class XmlEnvironment
    {
        public SchemasEnvironment Schemas { get; }
        public TypesEnvironment Types { get; }
        public ValuesEnvironment Values { get; }

        public XmlEnvironment(
            SchemasEnvironment schemas,
            TypesEnvironment types,
            ValuesEnvironment values)
        {
            Schemas = schemas;
            Types = types;
            Values = values;
        }
    }
}
