// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Generations.CodeGenerations.Exceptions
{
    internal sealed class CodeGeneratorConversionException : Exception
    {
        private readonly Type _sourceType;
        private readonly Type _targetType;
        private readonly bool _isAddress;
        private readonly string _reason;

        public CodeGeneratorConversionException(Type sourceType, Type targetType, bool isAddress, string reason)
            : base(SR.Format(SR.CodeGenConvertError, reason, sourceType.ToString(), targetType.ToString()))
        {
            _sourceType = sourceType;
            _targetType = targetType;
            _isAddress = isAddress;
            _reason = reason;
        }
    }
}
