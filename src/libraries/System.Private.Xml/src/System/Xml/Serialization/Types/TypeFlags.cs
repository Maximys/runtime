// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Types
{
    internal enum TypeFlags
    {
        None = 0,
        Abstract = 1,
        Reference = 1<<1,
        Special = 1<<2,
        CanBeAttributeValue = 1<<3,
        CanBeTextValue = 1<<4,
        CanBeElementValue = 1<<5,
        HasCustomFormatter = 1<<6,
        AmbiguousDataType = 1<<7,
        IgnoreDefault = 1<<9,
        HasIsEmpty = 1<<10,
        HasDefaultConstructor = 1<<11,
        XmlEncodingNotRequired = 1<<12,
        UseReflection = 1<<14,
        CollapseWhitespace = 1<<15,
        OptionalValue = 1<<16,
        CtorInaccessible = 1<<17,
        UsePrivateImplementation = 1<<18,
        GenericInterface = 1<<19,
        Unsupported = 1<<20,
    }
}
