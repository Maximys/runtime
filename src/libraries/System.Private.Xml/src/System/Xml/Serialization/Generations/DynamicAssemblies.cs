// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Reflection;

namespace System.Xml.Serialization.CodeGenerations
{
    internal static class DynamicAssemblies
    {
        private static readonly Hashtable s_nameToAssemblyMap = new Hashtable();
        private static readonly Hashtable s_assemblyToNameMap = new Hashtable();
        private static readonly ContextAwareTables<object> s_tableIsTypeDynamic = new ContextAwareTables<object>();

        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        internal static bool IsTypeDynamic(Type type)
        {
            object oIsTypeDynamic = s_tableIsTypeDynamic.GetOrCreateValue(type, static type =>
            {
                Assembly assembly = type.Assembly;
                bool isTypeDynamic = assembly.IsDynamic /*|| string.IsNullOrEmpty(assembly.Location)*/;
                if (!isTypeDynamic)
                {
                    if (type.IsArray)
                    {
                        isTypeDynamic = IsTypeDynamic(type.GetElementType()!);
                    }
                    else if (type.IsGenericType)
                    {
                        Type[] parameterTypes = type.GetGenericArguments();
                        if (parameterTypes != null)
                        {
                            for (int i = 0; i < parameterTypes.Length; i++)
                            {
                                Type parameterType = parameterTypes[i];
                                if (!(parameterType == null || parameterType.IsGenericParameter))
                                {
                                    isTypeDynamic = IsTypeDynamic(parameterType);
                                    if (isTypeDynamic)
                                        break;
                                }
                            }
                        }
                    }
                }
                return isTypeDynamic;
            });
            return (bool)oIsTypeDynamic;
        }

        internal static bool IsTypeDynamic(Type[] arguments)
        {
            foreach (Type t in arguments)
            {
                if (DynamicAssemblies.IsTypeDynamic(t))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void Add(Assembly a)
        {
            lock (s_nameToAssemblyMap)
            {
                if (s_assemblyToNameMap[a] != null)
                {
                    //already added
                    return;
                }
                Assembly? oldAssembly = s_nameToAssemblyMap[a.FullName!] as Assembly;
                string? key = null;
                if (oldAssembly == null)
                {
                    key = a.FullName;
                }
                else if (oldAssembly != a)
                {
                    //more than one assembly with same name
                    key = $"{a.FullName}, {s_nameToAssemblyMap.Count}";
                }
                if (key != null)
                {
                    s_nameToAssemblyMap.Add(key, a);
                    s_assemblyToNameMap.Add(a, key);
                }
            }
        }

        internal static Assembly? Get(string fullName)
        {
            return s_nameToAssemblyMap != null ? (Assembly?)s_nameToAssemblyMap[fullName] : null;
        }

        internal static string? GetName(Assembly a)
        {
            return s_assemblyToNameMap != null ? (string?)s_assemblyToNameMap[a] : null;
        }
    }
}
