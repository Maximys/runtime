// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Xml.Serialization.Mappings.AccessorMappings
{
    internal sealed class MemberMapping : AccessorMapping
    {
        private string? _name;

        internal MemberMapping() { }

        private MemberMapping(MemberMapping mapping)
            : base(mapping)
        {
            _name = mapping._name;
            CheckShouldPersist = mapping.CheckShouldPersist;
            CheckSpecified = mapping.CheckSpecified;
            IsReturnValue = mapping.IsReturnValue;
            ReadOnly = mapping.ReadOnly;
            SequenceId = mapping.SequenceId;
            MemberInfo = mapping.MemberInfo;
            CheckSpecifiedMemberInfo = mapping.CheckSpecifiedMemberInfo;
            CheckShouldPersistMethodInfo = mapping.CheckShouldPersistMethodInfo;
        }

        internal bool CheckShouldPersist { get; set; }

        internal SpecifiedAccessor CheckSpecified { get; set; }

        internal string Name
        {
            get { return _name ?? string.Empty; }
            set { _name = value; }
        }

        internal MemberInfo? MemberInfo{ get; set; }

        internal MemberInfo? CheckSpecifiedMemberInfo { get; set; }

        internal MethodInfo? CheckShouldPersistMethodInfo { get; set; }

        internal bool IsReturnValue { get; set; }

        internal bool ReadOnly { get; set; }

        internal bool IsSequence
        {
            get { return SequenceId >= 0; }
        }

        internal int SequenceId { get; set; } = -1;

        internal MemberMapping Clone()
        {
            return new MemberMapping(this);
        }
    }
}
