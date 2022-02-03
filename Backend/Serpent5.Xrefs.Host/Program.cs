#pragma warning disable CA1812 // Avoid uninstantiated internal classes

using System.Globalization;
using System.Text;
using AngleSharp;
using AngleSharp.Html;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Serpent5.Xrefs;
using Serpent5.Xrefs.Host.Extensions;

var webApplicationBuilder = WebApplication.CreateBuilder();

webApplicationBuilder.Services.AddMemoryCache();
webApplicationBuilder.Services.AddResponseCompression();
webApplicationBuilder.Services.AddHealthChecks();
webApplicationBuilder.Services.AddControllers();

webApplicationBuilder.Services.Configure<KestrelServerOptions>(static o =>
{
    o.AddServerHeader = false;
});

webApplicationBuilder.Services.Configure<HstsOptions>(static o =>
{
    o.MaxAge = TimeSpan.FromSeconds(63072000); // 2 Years.
    o.IncludeSubDomains = true;
});

webApplicationBuilder.Services.Configure<RouteOptions>(static o =>
{
    o.LowercaseUrls = true;
});

webApplicationBuilder.Services.AddOptions<StaticFileOptions>()
    .Configure<IHostEnvironment>(static (o, hostEnvironment) =>
    {
        o.ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
        {
            [".css"] = "text/css; charset=utf-8",
            [".ico"] = "image/x-icon",
            [".js"] = "application/javascript; charset=utf-8"
        });

        if (!hostEnvironment.IsDevelopment())
        {
            o.OnPrepareResponse = ctx =>
            {
                var immutableFileExtensions = new[] { ".css", ".js" };
                var fileExtension = Path.GetExtension(ctx.File.Name);

                if (immutableFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    var responseHeaders = ctx.Context.Response.Headers;

                    responseHeaders.CacheControl = "max-age=31536000,immutable"; // 1 Year.
                    responseHeaders.ETag = StringValues.Empty;
                    responseHeaders.LastModified = StringValues.Empty;
                }
            };
        }
    });

webApplicationBuilder.Services.Configure<HealthCheckOptions>(static o =>
{
    o.AllowCachingResponses = false;
    o.ResponseWriter = static (_, _) => Task.CompletedTask;

    o.ResultStatusCodes[HealthStatus.Healthy] = StatusCodes.Status204NoContent;
    o.ResultStatusCodes[HealthStatus.Degraded] = StatusCodes.Status204NoContent;
});

webApplicationBuilder.Services.AddSingleton<HttpClient>();
webApplicationBuilder.Services.AddSingleton<XrefClient>();

await using var webApplication = webApplicationBuilder.Build();

if (!webApplication.Environment.IsDevelopment())
{
    webApplication.UseHsts();
}

webApplication.UseWhen(
    static ctx => ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    static apiApplicationBuilder =>
    {
        apiApplicationBuilder.Use(static (ctx, nextMiddleware) =>
        {
            if (!ctx.Request.IsHttps)
            {
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Task.CompletedTask;
            }

            return nextMiddleware(ctx);
        });
    });

webApplication.UseHttpsRedirection();

webApplication.Use(static (ctx, nextMiddleware) =>
{
    ctx.Response.OnStarting(static ctxAsObject =>
    {
        var ctx = (HttpContext)ctxAsObject;

        if (ctx.Response.GetTypedHeaders().ContentType?.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase) == true)
        {
            var webHostEnvironment = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();

            var cspBuilder = new StringBuilder(
                "base-uri 'self'; " +
                "frame-ancestors 'none'; " +
                "object-src 'none'; ");

            if (webHostEnvironment.IsDevelopment())
            {
                cspBuilder.Append(
                    CultureInfo.InvariantCulture,
                    $"script-src 'strict-dynamic' 'nonce-{ctx.GetCspNonce()}' 'unsafe-inline' 'unsafe-eval' http: https:;");
            }
            else
            {
                cspBuilder.Append("require-trusted-types-for 'script';");
                cspBuilder.Append(
                    CultureInfo.InvariantCulture,
                    $"script-src 'strict-dynamic' 'nonce-{ctx.GetCspNonce()}' 'unsafe-inline' http: https:; ");
            }

            ctx.Response.Headers["Permissions-Policy"] = "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=()";
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }

        ctx.Response.Headers.XContentTypeOptions = "nosniff";

        return Task.CompletedTask;
    }, ctx);

    return nextMiddleware(ctx);
});

webApplication.Use(static (ctx, nextMiddleware) =>
{
    ctx.Response.OnStarting(static ctxAsObject =>
    {
        var ctx = (HttpContext)ctxAsObject;
        var httpResponseHeaders = ctx.Response.GetTypedHeaders();

        // It's common to set "Cache-Control: no-cache, no-store".
        // According to MDN (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control):
        // - no-store means "Don't cache this, ever".
        // - no-cache means "Check with me before you use the cache".
        // Because this doesn't seem logical, let's clear the value and let the "no-store" fallback kick in.
        if (httpResponseHeaders.CacheControl is { NoCache: true, NoStore: true })
            httpResponseHeaders.CacheControl = null;

        // Prefer ETag over Last-Modified.
        if (httpResponseHeaders.ETag is not null)
            httpResponseHeaders.LastModified = null;

        // Set a default Cache-Control header
        if (httpResponseHeaders.ETag is not null || httpResponseHeaders.LastModified is not null)
            // Resources with a version identifier should be checked first.
            httpResponseHeaders.CacheControl ??= new() { NoCache = true };
        else
            // All other resources aren't cacheable.
            httpResponseHeaders.CacheControl ??= new() { NoStore = true };

        // The Cache-Control header supercedes Expires and Pragma.
        ctx.Response.Headers.Remove(HeaderNames.Expires);
        ctx.Response.Headers.Remove(HeaderNames.Pragma);

        return Task.CompletedTask;
    }, ctx);

    return nextMiddleware(ctx);
});

webApplication.Use(static (ctx, nextMiddleware) =>
{
    // 404 for explicit requests to /index.html.
    if (ctx.Request.Path == "/index.html")
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }

    return nextMiddleware(ctx);
});

webApplication.UseResponseCompression();
webApplication.UseStaticFiles();

webApplication.MapHealthChecks("/healthz");
webApplication.MapControllers();

webApplication.MapFallback(static async (HttpContext ctx, IMemoryCache memoryCache) =>
{
    var (httpRequest, httpResponse, cancellationToken) = (ctx.Request, ctx.Response, ctx.RequestAborted);

    if (httpRequest.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrEmpty(Path.GetExtension(httpRequest.Path)))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    const string indexFilename = "index.html";

    var htmlContent = await memoryCache.GetOrCreateAsync<string?>(indexFilename, async cacheEntry =>
    {
        var webHostEnvironment = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var webRootFileProvider = webHostEnvironment.WebRootFileProvider;
        var indexFileInfo = webRootFileProvider?.GetFileInfo(indexFilename);

        if (indexFileInfo is null || !indexFileInfo.Exists)
            return null;

        if (webHostEnvironment.IsDevelopment() && webRootFileProvider is not null)
            cacheEntry.AddExpirationToken(webRootFileProvider.Watch(indexFilename));

        using var streamReader = new StreamReader(indexFileInfo.CreateReadStream());

        return await streamReader.ReadToEndAsync();
    });

    if (htmlContent is null)
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    using var browsingContext = BrowsingContext.New();
    using var htmlDocument = await browsingContext.OpenAsync(
        virtualResponse => virtualResponse.Content(htmlContent),
        cancellationToken);

    var cspNonce = ctx.GetCspNonce();

    foreach (var htmlScriptElement in htmlDocument.Scripts)
        htmlScriptElement.SetAttribute("nonce", cspNonce);

    var htmlContentBytes = Encoding.UTF8.GetBytes(htmlDocument.ToHtml(new MinifyMarkupFormatter()));

    httpResponse.ContentLength = htmlContentBytes.Length;
    httpResponse.ContentType = "text/html; charset=utf-8";

    await httpResponse.Body.WriteAsync(htmlContentBytes, cancellationToken);
});

await webApplication.RunAsync();

#pragma warning restore CA1812 // Avoid uninstantiated internal classes
