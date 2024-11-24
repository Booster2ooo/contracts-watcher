using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using System.Windows;

namespace ContractsWatcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? webHost; 
        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = WebApplication.CreateBuilder(e.Args);
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
            webHost = app;
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
                .UseHttpsRedirection()
                .UseAuthorization()
                .UseResponseCompression()
                .UseBlazorFrameworkFiles()
                .UseStaticFiles()
                ;

            app.MapHub<ContractsHub>("/contracts-hub");
            app.MapFallbackToFile("index.html");

            app.StartAsync();
            webHost.Services.GetService<IHostApplicationLifetime>()?.ApplicationStopping.Register(OnAppStopping);
        }

        private void OnAppStopping()
        {
            webHost?.Dispose();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            webHost?.StopAsync();
            base.OnExit(e);
        }
    }

}
