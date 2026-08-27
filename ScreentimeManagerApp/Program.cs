
using ScreentimeManagerCore.Configuration;
using ScreentimeManagerCore.Interfaces;
using ScreentimeManagerCore.Services;
using Serilog;

namespace ScreentimeManagerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configuration Setup
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            // Setup logging with Serilog to get config from config files and write to console
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSerilog();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Bind configuration
            builder.Services.AddOptions<OnlineCheckConfiguration>()
                .Bind(configuration)
                .ValidateOnStart();
            builder.Services.AddOptions<RemoteShutdownConfiguration>()
                .Bind(configuration)
                .ValidateOnStart();
            builder.Services.AddOptions<ScreentimeCounterConfiguration>()
                .Bind(configuration)
                .ValidateOnStart();

            // Add background services to the DI container
            builder.Services.AddSingleton<IRemoteShutdownService, NetRemoteShutdownService>();
            // Add IHostedService in a way that allows cross-service DI
            builder.Services.AddSingleton<IHostOnlineCheckService, PingHostOnlineCheckService>();
            builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<IHostOnlineCheckService>());
            // Add IHostedService in a way that allows cross-service DI
            builder.Services.AddSingleton<ScreentimeCounterService>();
            builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<ScreentimeCounterService>());
            // Add IHostedService in a way that allows cross - service DI
            builder.Services.AddSingleton<ScreentimeManagerStatemachine>();
            builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<ScreentimeManagerStatemachine>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
    