using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Perifericos.Agent.Services;

namespace Perifericos.Agent;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "agent-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient();
                    services.Configure<AgentOptions>(context.Configuration.GetSection("Agent"));
                    services.AddSingleton<PopupNotifier>();
                    services.AddSingleton<AuthorizedDevicesCache>();
                    services.AddHostedService<UsbMonitorService>();
                    services.AddSingleton<ApiClient>();
                })
                .Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Agente falhou na inicialização");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

