#pragma warning disable CA1812 // Avoid uninstantiated internal classes

using Serpent5.Xrefs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<XrefClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

#pragma warning restore CA1812 // Avoid uninstantiated internal classes
