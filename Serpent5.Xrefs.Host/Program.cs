#pragma warning disable CA1812 // Avoid uninstantiated internal classes

using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Serpent5.Xrefs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddResponseCompression();
builder.Services.AddControllers();

builder.Services.Configure<KestrelServerOptions>(static o =>
{
    o.AddServerHeader = false;
});

builder.Services.Configure<HstsOptions>(static o =>
{
    o.MaxAge = TimeSpan.FromSeconds(63072000); // 2 Years.
    o.IncludeSubDomains = true;
});

builder.Services.AddOptions<StaticFileOptions>()
    .Configure<IHostEnvironment>(static (o, hostEnvironment) =>
    {
        o.ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
        {
            [".css"] = "text/css; charset=utf-8",
            [".ico"] = "image/x-icon",
            [".js"] = "application/javascript; charset=utf-8",
            [".txt"] = "text/plain; charset=utf-8"
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

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<XrefClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(static (ctx, nextMiddleware) =>
{
    ctx.Response.OnStarting(static ctxAsObject =>
    {
        var ctx = (HttpContext)ctxAsObject;

        if (ctx.Response.GetTypedHeaders().ContentType?.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase) == true)
        {
            ctx.Response.Headers["Permissions-Policy"] = "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=()";
            ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            ctx.Response.Headers[HeaderNames.XFrameOptions] = "DENY";
        }

        ctx.Response.Headers.XContentTypeOptions = "nosniff";

        return Task.CompletedTask;
    }, ctx);

    return nextMiddleware(ctx);
});

app.Use(static (ctx, nextMiddleware) =>
{
    // 404 for explicit requests to /index.html.
    if (ctx.Request.Path == "/index.html")
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }

    return nextMiddleware(ctx);
});

app.UseResponseCompression();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

#pragma warning restore CA1812 // Avoid uninstantiated internal classes
