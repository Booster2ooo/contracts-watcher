using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.MapControllers();
app.UseCors(builder =>
    {
        builder.SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    })
    .UseAuthorization()
    .UseResponseCompression()
    .UseBlazorFrameworkFiles()
    .UseStaticFiles()
    ;

app.MapHub<ContractsHub>("/contracts-hub");
app.MapFallbackToFile("index.html");

await app.RunAsync();
