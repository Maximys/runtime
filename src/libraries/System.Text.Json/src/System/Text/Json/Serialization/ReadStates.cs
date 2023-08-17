// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json
{
    [Flags]
    internal enum ReadStates
    {
        None    = MetadataPropertyName.None,
        Values  = MetadataPropertyName.Values,
        Id      = MetadataPropertyName.Id,
        Ref     = MetadataPropertyName.Ref,
        Type    = MetadataPropertyName.Type,
    }
}
