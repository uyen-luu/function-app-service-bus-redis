using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace IPS.Grow.Web.Server.Extensions;

public class ConfigFactory
{
    public static void ConfigureServer(KestrelServerOptions options)
    {
        options.AddServerHeader = false;
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        options.Limits.MaxConcurrentConnections = 100;
        options.Limits.MaxConcurrentUpgradedConnections = 100;
        options.Limits.MaxRequestBodySize = 100_000_000;
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        options.Limits.Http2.MaxStreamsPerConnection = 100;
        options.Limits.Http2.MaxRequestHeaderFieldSize = 8192;
        options.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(30);
        options.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromMinutes(1);
        options.AllowSynchronousIO = false;
    }

    public static void ConfigureJson(JsonOptions options)
    {

    }
}
