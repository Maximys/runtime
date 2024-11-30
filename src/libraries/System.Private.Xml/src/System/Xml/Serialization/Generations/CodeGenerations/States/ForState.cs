// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;

namespace System.Xml.Serialization.Generations.CodeGenerations.States
{
    internal sealed class ForState
    {
        internal LocalBuilder Index{ get; }

        internal Label BeginLabel{ get; }

        internal Label TestLabel{ get; }

        internal object End{ get; }

        internal ForState(LocalBuilder index, Label beginLabel, Label testLabel, object end)
        {
            Index = index;
            BeginLabel = beginLabel;
            TestLabel = testLabel;
            End = end;
        }
    }
}
