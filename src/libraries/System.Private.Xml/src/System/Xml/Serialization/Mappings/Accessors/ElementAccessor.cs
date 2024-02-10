// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings.Accessors
{
    internal sealed class ElementAccessor : Accessor
    {
        internal bool IsSoap { get; set; }

        internal bool IsNullable { get; set; }

        internal bool IsUnbounded {  get; set; }

        internal ElementAccessor Clone()
        {
            ElementAccessor newAccessor = new ElementAccessor();

            newAccessor.IsNullable = IsNullable;
            newAccessor.IsTopLevelInSchema = IsTopLevelInSchema;
            newAccessor.Form = Form;
            newAccessor.IsSoap = IsSoap;
            newAccessor.Name = Name;
            newAccessor.Default = Default;
            newAccessor.Namespace = Namespace;
            newAccessor.Mapping = Mapping;
            newAccessor.Any = Any;

            return newAccessor;
        }
    }
}
