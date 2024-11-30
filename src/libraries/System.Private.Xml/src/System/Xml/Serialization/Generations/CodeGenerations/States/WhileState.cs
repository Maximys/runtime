// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;

namespace System.Xml.Serialization.Generations.CodeGenerations.States
{
    internal sealed class WhileState
    {
        public Label StartLabel { get; }
        public Label CondLabel { get; }
        public Label EndLabel { get; }

        public WhileState(CodeGenerator ilg)
        {
            StartLabel = ilg.DefineLabel();
            CondLabel = ilg.DefineLabel();
            EndLabel = ilg.DefineLabel();
        }
    }
}
