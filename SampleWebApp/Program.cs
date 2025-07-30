using Microsoft.AspNetCore.ResponseCompression;
using SampleWebApp;
using log4net;
var builder = WebApplication.CreateBuilder(args);
// Register services
builder.Services.AddScoped<MockSchemaService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Logging.AddLog4Net("log4net.config");
builder.Services.AddControllers();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseResponseCompression();

// Add UseRouting() if you need explicit routing setup, otherwise Map* methods handle it.
// If you were using app.UseRouting() explicitly, it would go here.

app.UseStatusCodePagesWithReExecute("/not-found");

// IMPORTANT: Place UseAntiforgery() AFTER UseRouting() (implicitly done by Map* methods)
// and before your endpoint definitions (MapBlazorHub, MapStaticAssets, MapRazorComponents, MapControllers).
app.UseAntiforgery(); // THIS IS THE CORRECT PLACEMENT

// Endpoint mappings
//app.MapBlazorHub(); // Uncomment if you are using BlazorHub directly
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
