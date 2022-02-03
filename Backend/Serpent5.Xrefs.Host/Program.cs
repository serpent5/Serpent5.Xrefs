#pragma warning disable  // Avoid uninstantiated internal classes

using Serpent5.Xrefs;

var webApplicationBuilder = WebApplication.CreateBuilder();

webApplicationBuilder.Services.AddSingleton<HttpClient>();
webApplicationBuilder.Services.AddSingleton<XrefClient>();

webApplicationBuilder.Services.AddRazorPages();

webApplicationBuilder.Services.Configure<RouteOptions>(o =>
{
    o.AppendTrailingSlash = true;
    o.LowercaseUrls = true;
});

await using var webApplication = webApplicationBuilder.Build();

webApplication.MapRazorPages();

await webApplication.RunAsync();

#pragma warning restore CA1812 // Avoid uninstantiated internal classes
