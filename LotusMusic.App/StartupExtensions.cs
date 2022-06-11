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
using Microsoft.AspNetCore.Builder;
using Discord.WebSocket;

namespace LotusMusic.App;

internal static class StartupExtensions
{

    internal static void AddHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
#if !DEBUG
            AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());
            options.UseSqlite(context.Configuration.GetConnectionString("Default"));
#else
            options.UseInMemoryDatabase($"Debug");
#endif

        });
        services.AddHostedService<CommandHandler>();
        services.AddHostedService<StatusHandler>();
    }
    internal static void AddNetworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLavaNode(x =>
        {
            x.SelfDeaf = false;
            x.Hostname = "lava.link";
            x.Port = 80;
        });
    }

    internal static void AddSingletons(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ILocalSource, LocalSource>();
        services.AddSingleton<IAudioPlayer, LavalinkAudio>();
        services.AddSingleton<IPageResolver, PageResolver>();
    }
}
