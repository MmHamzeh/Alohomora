using System;
using System.Collections.Generic;
using System.Text;

namespace Alohomora.Common.Configs;

internal class EasyCachingConfigs
{
    public const string AccessTokenIdStoreHost = "http://localhost";
    public const int AccessTokenIdStorePort = 6379;
    public const string AccessTokenIdStoreName = "AccessTokenIdStore";
}
