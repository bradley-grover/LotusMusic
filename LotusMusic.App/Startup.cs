using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using LotusMusic.App.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Victoria;
using LotusMusic.Core.Local;
using LotusMusic.Core.Paging;

namespace LotusMusic.App;

internal static class Startup
{
    public static IHostBuilder CreateHostBuilder() =>
        new HostBuilder()
        .ConfigureAppConfiguration(config =>
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            config.AddConfiguration(configuration);
        })
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
        .ConfigureServices((context, services) =>
        {
            services.AddHostedServices(context.Configuration);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
#if !DEBUG
                AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());
                options.UseSqlite(context.Configuration.GetConnectionString("Default"));
#else
                options.UseInMemoryDatabase($"Debug");
#endif

            });
            services.AddNetworkServices(context.Configuration);
            services.AddSingletons(context.Configuration);
            
        })
        .UseConsoleLifetime();

    internal static void AddHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<CommandHandler>();
        services.AddHostedService<StatusHandler>();
    }
    internal static void AddNetworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLavaNode(x =>
        {
            x.Hostname = "lava.link";
            x.Port = 80;
            x.SelfDeaf = false;
        });
    }
    internal static void AddSingletons(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ILocalSource, LocalSource>();
        services.AddSingleton<IAudioPlayer, LavalinkAudio>();
        services.AddSingleton<IPageResolver, PageResolver>();
    }

    public static string GetToken(IConfiguration config)
    {
        if (config.GetSection("Client-Configuration").GetValue<bool>("Use-Test-Token"))
        {
            return Environment.GetEnvironmentVariable("DISCORD_TOKEN_DEBUG") ?? throw new InvalidOperationException("No debug token set");
        }
        return Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("No discord bot token is set");
    }
}
