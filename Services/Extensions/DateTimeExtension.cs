// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Extensions
{
    public static class DateTimeExtension
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToEpochMilli(this DateTime time)
        {
            return (long)(time - epoch).TotalMilliseconds;
        }
    }
}