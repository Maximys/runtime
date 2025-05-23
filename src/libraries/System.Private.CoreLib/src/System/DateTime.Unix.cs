// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System
{
    public readonly partial struct DateTime
    {
        internal static bool SystemSupportsLeapSeconds => false;

        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(((ulong)(Interop.Sys.GetSystemTimeAsTicks() + UnixEpochTicks)) | KindUtc);
            }
        }

#pragma warning disable IDE0060

        // Never called
        private static DateTime FromFileTimeLeapSecondsAware(ulong fileTime) => default;

        // Never called
        private static ulong ToFileTimeLeapSecondsAware(long ticks) => default;

        // IsValidTimeWithLeapSeconds is not expected to be called at all for now on non-Windows platforms
        internal static bool IsValidTimeWithLeapSeconds(DateTime value) => false;

#pragma warning restore IDE0060
    }
}
