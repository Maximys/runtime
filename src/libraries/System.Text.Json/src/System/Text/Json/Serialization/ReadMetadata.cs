// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json
{
    internal struct ReadMetadata
    {
        /// <summary>
        /// Holds the value of $type.
        /// </summary>
        public object? PolymorphicTypeDiscriminator { get; set; }

        /// <summary>
        /// Holds the value of $id or $ref.
        /// </summary>
        public string? ReferenceId { get; set; }
    }
}
