using ContractsWatcher.Components;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddElectron();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services
    .AddOpenApi()
    .AddApplicationConfig(builder.Configuration)
    .AddApplicationServices()
    .AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
    })
    ;


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
else
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "api/doc";
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

//app.UseHttpsRedirection();


app.UseAntiforgery();
app.UseAuthorization();
app.UseCors(builder =>
{
    builder.SetIsOriginAllowed((host) => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});
app.MapControllers();
app.MapHub<ContractsHub>("/hubs/contracts");
app.MapStaticAssets();
app.MapRazorComponents<ContractsWatcher.Components.App>()
    .AddInteractiveServerRenderMode();
app.UseResponseCompression();


await app.StartAsync();

try
{
    var mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions()
    {
        AlwaysOnTop = true,
        Transparent = true,
        Frame = false
    }); ;
}
catch { }
app.WaitForShutdown();