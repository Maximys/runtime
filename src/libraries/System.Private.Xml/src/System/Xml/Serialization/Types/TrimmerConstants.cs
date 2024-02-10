// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization.Types
{
    /// <summary>
    /// Shorthands for common trimmer constants.
    /// </summary>
    internal static class TrimmerConstants
    {
        public const DynamicallyAccessedMemberTypes AllMethods = DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors;
        public const DynamicallyAccessedMemberTypes PublicMembers = DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicNestedTypes
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicEvents;
    }
}
