// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;

namespace System.Xml.Serialization.Generations.Building
{
    internal sealed class MethodBuilderInfo
    {
        public readonly MethodBuilder MethodBuilder;
        public readonly Type[] ParameterTypes;
        public MethodBuilderInfo(MethodBuilder methodBuilder, Type[] parameterTypes)
        {
            MethodBuilder = methodBuilder;
            ParameterTypes = parameterTypes;
        }

        [Conditional("DEBUG")]
        public void Validate(Type? returnType, Type[] parameterTypes, MethodAttributes attributes)
        {
            Debug.Assert(MethodBuilder.ReturnType == returnType);
            Debug.Assert(MethodBuilder.Attributes == attributes);
            Debug.Assert(ParameterTypes.Length == parameterTypes.Length);
            for (int i = 0; i < parameterTypes.Length; ++i)
            {
                Debug.Assert(ParameterTypes[i] == parameterTypes[i]);
            }
        }
    }
}
