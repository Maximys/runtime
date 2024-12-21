// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Environments.Schemas
{
    internal sealed class SchemasEnvironment
    {
        public required InstancesEnvironment Instances { get; init; }
        public required string NamespaceId { get; init; }
        public required string Namespace1999Id { get; init; }
        public required string Namespace2000Id { get; init; }
        public required string NonXsdTypesNamespaceId { get; init; }
        public required string SoapNamespaceId { get; init; }
        public required string Soap12NamespaceId { get; init; }
    }
}
