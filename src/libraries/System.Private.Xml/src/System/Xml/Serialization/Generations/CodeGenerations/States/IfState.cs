// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;

namespace System.Xml.Serialization.Generations.CodeGenerations.States
{
    internal sealed class IfState
    {
        internal Label EndIf { get; set; }

        internal Label ElseBegin { get; set; }
    }
}
