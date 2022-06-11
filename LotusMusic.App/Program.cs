using Discord;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Discord.Addons.Hosting;

namespace LotusMusic.App;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new()
                {
                    LogLevel = LogSeverity.Debug,
                    AlwaysDownloadUsers = false,
                    MessageCacheSize = 0XC8
                };
                config.Token = GetToken(context.Configuration);
            })
            .UseCommandService((context, config) =>
            {
                config.CaseSensitiveCommands = false;
                config.LogLevel = LogSeverity.Debug;
                config.DefaultRunMode = RunMode.Async;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseDefaultServiceProvider(x =>
                {
                    x.ValidateOnBuild = true;
                });
            });


    private static string GetToken(IConfiguration configuration)
    {
        if (configuration.GetSection("Client-Configuration").GetValue<bool>("Use-Test-Token"))
        {
            return Environment.GetEnvironmentVariable("DISCORD_TOKEN_DEBUG") ?? throw new InvalidOperationException("No debug token set");
        }
        return Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("No discord bot token is set");
    }
}
