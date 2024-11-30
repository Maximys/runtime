// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Xml.Serialization.Generations.Building;
using System.Xml.Serialization.Generations.CodeGenerations;
using System.Xml.Serialization.Mappings.AccessorMappings;
using System.Xml.Serialization.Types;
using System.Collections.Generic;

namespace System.Xml.Serialization.Generations.IntermediateLanguageGenerations
{
    internal sealed class ReflectionAwareILGen
    {
        // reflectionVariables holds mapping between a reflection entity
        // referenced in the generated code (such as TypeInfo,
        // FieldInfo) and the variable which represent the entity (and
        // initialized before).
        // The types of reflection entity and corresponding key is
        // given below.
        // ----------------------------------------------------------------------------------
        // Entity           Key
        // ----------------------------------------------------------------------------------
        // Assembly         assembly.FullName
        // Type             CodeIdentifier.EscapedKeywords(type.FullName)
        // Field            fieldName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName>)
        // Property         propertyName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName)
        // ArrayAccessor    "0:"+CodeIdentifier.EscapedKeywords(typeof(Array).FullName)
        // MyCollectionAccessor     "0:"+CodeIdentifier.EscapedKeywords(typeof(MyCollection).FullName)
        // ----------------------------------------------------------------------------------
        internal ReflectionAwareILGen() { }

        [RequiresUnreferencedCode("calls GetTypeDesc")]
        internal static void WriteReflectionInit(TypeScope scope)
        {
            foreach (Type type in scope.Types)
            {
                scope.GetTypeDesc(type);
            }
        }

        internal static void ILGenForEnumLongValue(CodeGenerator ilg, string variable)
        {
            ArgBuilder argV = ilg.GetArg(variable);
            ilg.Ldarg(argV);
            ilg.ConvertValue(argV.ArgType, typeof(long));
        }

        internal static string GetStringForTypeof(string typeFullName)
        {
            return $"typeof({typeFullName})";
        }
        internal static string GetStringForMember(string obj, string memberName)
        {
            return $"{obj}.@{memberName}";
        }
        internal static SourceInfo GetSourceForMember(string obj, MemberMapping member, CodeGenerator ilg)
        {
            return GetSourceForMember(obj, member, member.MemberInfo, ilg);
        }
        internal static SourceInfo GetSourceForMember(string obj, MemberMapping member, MemberInfo? memberInfo, CodeGenerator ilg)
        {
            return new SourceInfo(GetStringForMember(obj, member.Name), obj, memberInfo, member.TypeDesc!.Type, ilg);
        }

        internal static void ILGenForEnumMember(CodeGenerator ilg, Type type, string memberName)
        {
            ilg.Ldc(Enum.Parse(type, memberName, false));
        }
        internal static string GetStringForArrayMember(string? arrayName, string subscript)
        {
            return $"{arrayName}[{subscript}]";
        }
        internal static string GetStringForMethod(string obj, string memberName)
        {
            return $"{obj}.{memberName}(";
        }

        [RequiresUnreferencedCode("calls ILGenForCreateInstance")]
        internal static void ILGenForCreateInstance(CodeGenerator ilg, Type type, bool ctorInaccessible, bool cast)
        {
            if (!ctorInaccessible)
            {
                ConstructorInfo ctor = type.GetConstructor(
                       CodeGenerator.InstanceBindingFlags,
                       Type.EmptyTypes
                       )!;
                if (ctor != null)
                    ilg.New(ctor);
                else
                {
                    Debug.Assert(type.IsValueType);
                    LocalBuilder tmpLoc = ilg.GetTempLocal(type);
                    ilg.Ldloca(tmpLoc);
                    ilg.InitObj(type);
                    ilg.Ldloc(tmpLoc);
                }
                return;
            }
            ILGenForCreateInstance(ilg, type, cast ? type : null);
        }

        [RequiresUnreferencedCode("calls GetType")]
        internal static void ILGenForCreateInstance(CodeGenerator ilg, Type type, Type? cast)
        {
            // Special case DBNull
            if (type == typeof(DBNull))
            {
                FieldInfo DBNull_Value = type.GetField("Value", CodeGenerator.StaticBindingFlags)!;
                ilg.LoadMember(DBNull_Value);
                return;
            }

            // Special case XElement
            // codegen the same as 'internal XElement : this("default") { }'
            if (type.FullName == "System.Xml.Linq.XElement")
            {
                Type? xName = type.Assembly.GetType("System.Xml.Linq.XName");
                if (xName != null)
                {
                    MethodInfo XName_op_Implicit = xName.GetMethod(
                        "op_Implicit",
                        CodeGenerator.StaticBindingFlags,
                        new Type[] { typeof(string) }
                        )!;
                    ConstructorInfo XElement_ctor = type.GetConstructor(
                        CodeGenerator.InstanceBindingFlags,
                        new Type[] { xName }
                        )!;
                    if (XName_op_Implicit != null && XElement_ctor != null)
                    {
                        ilg.Ldstr("default");
                        ilg.Call(XName_op_Implicit);
                        ilg.New(XElement_ctor);
                        return;
                    }
                }
            }

            Label labelReturn = ilg.DefineLabel();
            Label labelEndIf = ilg.DefineLabel();

            // TypeInfo typeInfo = type.GetTypeInfo();
            // typeInfo not declared explicitly
            ilg.Ldc(type);
            MethodInfo getTypeInfoMehod = typeof(IntrospectionExtensions).GetMethod(
                  "GetTypeInfo",
                  CodeGenerator.StaticBindingFlags,
                  new[] { typeof(Type) }
                  )!;
            ilg.Call(getTypeInfoMehod);

            // IEnumerator<ConstructorInfo> e = typeInfo.DeclaredConstructors.GetEnumerator();
            LocalBuilder enumerator = ilg.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(typeof(ConstructorInfo)), "e");
            MethodInfo getDeclaredConstructors = typeof(TypeInfo).GetMethod("get_DeclaredConstructors")!;
            MethodInfo getEnumerator = typeof(IEnumerable<>).MakeGenericType(typeof(ConstructorInfo)).GetMethod("GetEnumerator")!;
            ilg.Call(getDeclaredConstructors);
            ilg.Call(getEnumerator);
            ilg.Stloc(enumerator);

            ilg.WhileBegin();
            // ConstructorInfo constructorInfo = e.Current();
            MethodInfo enumeratorCurrent = typeof(IEnumerator).GetMethod("get_Current")!;
            ilg.Ldloc(enumerator);
            ilg.Call(enumeratorCurrent);
            LocalBuilder constructorInfo = ilg.DeclareLocal(typeof(ConstructorInfo), "constructorInfo");
            ilg.Stloc(constructorInfo);

            // if (!constructorInfo.IsStatic && constructorInfo.GetParameters.Length() == 0)
            ilg.Ldloc(constructorInfo);
            MethodInfo constructorIsStatic = typeof(ConstructorInfo).GetMethod("get_IsStatic")!;
            ilg.Call(constructorIsStatic);
            ilg.Brtrue(labelEndIf);
            ilg.Ldloc(constructorInfo);
            MethodInfo constructorGetParameters = typeof(ConstructorInfo).GetMethod("GetParameters")!;
            ilg.Call(constructorGetParameters);
            ilg.Ldlen();
            ilg.Ldc(0);
            ilg.Cne();
            ilg.Brtrue(labelEndIf);

            // constructorInfo.Invoke(null);
            MethodInfo constructorInvoke = typeof(ConstructorInfo).GetMethod("Invoke", new Type[] { typeof(object[]) })!;
            ilg.Ldloc(constructorInfo);
            ilg.Load(null);
            ilg.Call(constructorInvoke);
            ilg.Br(labelReturn);

            ilg.MarkLabel(labelEndIf);
            ilg.WhileBeginCondition(); // while (e.MoveNext())
            MethodInfo IEnumeratorMoveNext = typeof(IEnumerator).GetMethod(
                "MoveNext",
                CodeGenerator.InstanceBindingFlags,
                Type.EmptyTypes)!;
            ilg.Ldloc(enumerator);
            ilg.Call(IEnumeratorMoveNext);
            ilg.WhileEndCondition();
            ilg.WhileEnd();

            MethodInfo Activator_CreateInstance = typeof(Activator).GetMethod(
                  "CreateInstance",
                  CodeGenerator.StaticBindingFlags,
                  new Type[] { typeof(Type) }
                  )!;
            ilg.Ldc(type);
            ilg.Call(Activator_CreateInstance);
            ilg.MarkLabel(labelReturn);
            if (cast != null)
                ilg.ConvertValue(Activator_CreateInstance.ReturnType, cast);
        }

        [RequiresUnreferencedCode("calls LoadMember")]
        internal static void WriteLocalDecl(string variableName, SourceInfo initValue)
        {
            Type localType = initValue.Type!;
            LocalBuilder localA = initValue.ILG.DeclareOrGetLocal(localType, variableName);
            if (initValue.Source != null)
            {
                if (initValue == "null")
                {
                    initValue.ILG.Load(null);
                }
                else
                {
                    if (initValue.Arg.StartsWith("o.@", StringComparison.Ordinal))
                    {
                        Debug.Assert(initValue.MemberInfo != null);
                        Debug.Assert(initValue.MemberInfo.Name == initValue.Arg.Substring(3));
                        initValue.ILG.LoadMember(initValue.ILG.GetLocal("o"), initValue.MemberInfo);
                    }
                    else if (initValue.Source.EndsWith(']'))
                    {
                        initValue.Load(initValue.Type);
                    }
                    else if (initValue.Source == "fixup.Source" || initValue.Source == "e.Current")
                    {
                        string[] vars = initValue.Source.Split('.');
                        object fixup = initValue.ILG.GetVariable(vars[0]);
                        PropertyInfo propInfo = CodeGenerator.GetVariableType(fixup).GetProperty(vars[1])!;
                        initValue.ILG.LoadMember(fixup, propInfo);
                        initValue.ILG.ConvertValue(propInfo.PropertyType, localA.LocalType);
                    }
                    else
                    {
                        object sVar = initValue.ILG.GetVariable(initValue.Arg);
                        initValue.ILG.Load(sVar);
                        initValue.ILG.ConvertValue(CodeGenerator.GetVariableType(sVar), localA.LocalType);
                    }
                }
                initValue.ILG.Stloc(localA);
            }
        }

        [RequiresUnreferencedCode("calls ILGenForCreateInstance")]
        internal static void WriteCreateInstance(string source, bool ctorInaccessible, Type type, CodeGenerator ilg)
        {
            LocalBuilder sLoc = ilg.DeclareOrGetLocal(type, source);
            ILGenForCreateInstance(ilg, type, ctorInaccessible, ctorInaccessible);
            ilg.Stloc(sLoc);
        }

        [RequiresUnreferencedCode("calls Load")]
        internal static void WriteInstanceOf(SourceInfo source, Type type, CodeGenerator ilg)
        {
            source.Load(typeof(object));
            ilg.IsInst(type);
            ilg.Load(null);
            ilg.Cne();
        }

        [RequiresUnreferencedCode("calls Load")]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        internal static void WriteArrayLocalDecl(string typeName, string variableName, SourceInfo initValue, TypeDesc arrayTypeDesc)
        {
            Debug.Assert(typeName == arrayTypeDesc.CSharpName || typeName == $"{arrayTypeDesc.CSharpName}[]");
            Type localType = typeName == arrayTypeDesc.CSharpName ? arrayTypeDesc.Type! : arrayTypeDesc.Type!.MakeArrayType();
            // This may need reused variable to get code compat?
            LocalBuilder local = initValue.ILG.DeclareOrGetLocal(localType, variableName);
            initValue.Load(local.LocalType);
            initValue.ILG.Stloc(local);
        }
        internal static void WriteTypeCompare(string variable, Type type, CodeGenerator ilg)
        {
            Debug.Assert(type != null);
            Debug.Assert(ilg != null);
            ilg.Ldloc(typeof(Type), variable);
            ilg.Ldc(type);
            ilg.Ceq();
        }
        internal static void WriteArrayTypeCompare(string variable, Type arrayType, CodeGenerator ilg)
        {
            Debug.Assert(arrayType != null);
            Debug.Assert(ilg != null);
            ilg.Ldloc(typeof(Type), variable);
            ilg.Ldc(arrayType);
            ilg.Ceq();
        }

        [return: NotNullIfNotNull(nameof(value))]
        internal static string? GetQuotedCSharpString(string? value) =>
            value is null ? null :
            $"@\"{GetCSharpString(value)}\"";

        [return: NotNullIfNotNull(nameof(value))]
        internal static string? GetCSharpString(string? value)
        {
            if (value == null)
            {
                return null;
            }

            // Find the first character to be escaped.
            int i;
            for (i = 0; i < value.Length; i++)
            {
                if (value[i] is < (char)32 or '\"')
                {
                    break;
                }
            }

            // If nothing needs to be escaped, return the original string.
            if (i == value.Length)
            {
                return value;
            }

            var builder = new ValueStringBuilder(stackalloc char[128]);

            // Copy over all text that needn't be escaped.
            builder.Append(value.AsSpan(0, i));

            // Process the remainder of the string, escaping each value that needs to be.
            for (; i < value.Length; i++)
            {
                char ch = value[i];

                if (ch < 32)
                {
                    if (ch == '\r')
                    {
                        builder.Append("\\r");
                    }
                    else if (ch == '\n')
                    {
                        builder.Append("\\n");
                    }
                    else if (ch == '\t')
                    {
                        builder.Append("\\t");
                    }
                    else
                    {
                        byte b = (byte)ch;
                        builder.Append("\\x");
                        builder.Append(HexConverter.ToCharUpper(b >> 4));
                        builder.Append(HexConverter.ToCharUpper(b));
                    }
                }
                else if (ch == '\"')
                {
                    builder.Append("\"\"");
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }
    }
}
