using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using Victoria;

namespace LotusMusic.App.Services;

internal class CommandHandler : DiscordClientService
{
    private InteractionService? Interactions { get; set; }
    private CommandService Service { get; }
    private ApplicationDbContext Context { get; }
    private IServiceProvider Provider { get; }
    private IConfiguration Configuration { get; }
    private LavaNode Node { get; }

    public CommandHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger,
        CommandService service, ApplicationDbContext context,
        IServiceProvider provider, IConfiguration configuration, LavaNode node) 
        : base(client, logger)
    {
        Service = service;
        Context = context;
        Provider = provider;
        Configuration = configuration;
        Node = node;
    }

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.Ready += Client_Ready;

        await Service.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
    }

    private async Task Client_Ready()
    {
        Interactions = new InteractionService(Client, new()
        {
            DefaultRunMode = Discord.Interactions.RunMode.Async,
            LogLevel = LogSeverity.Debug
        });

        await Interactions.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
#if DEBUG
        await Interactions.RegisterCommandsToGuildAsync(ulong.Parse(Configuration.GetSection("Client-Configuration")["Test-Server"]));
#else
        await Interactions.RegisterCommandsGloballyAsync();
#endif
        Client.InteractionCreated += async interaction =>
        {
            var context = new SocketInteractionContext(Client, interaction);
            await Interactions.ExecuteCommandAsync(context, Provider);
        };
        Client.ButtonExecuted += Client_ButtonExecuted;

        Logger.LogInformation("Attemping to connect to Lavalink");
        if (!Node.IsConnected)
        {
            await Node.ConnectAsync();
        }
        Logger.LogInformation("After connect");
    }

    private async Task Client_ButtonExecuted(SocketMessageComponent arg)
    {
        if (IsPagedEmbed(arg.Data.CustomId))
        {

        }
    }

    static bool IsPagedEmbed(ReadOnlySpan<char> value)
    {
        return value.Contains(InteractionEvents.ButtonLeft.AsSpan(), StringComparison.Ordinal)
            || value.Contains(InteractionEvents.ButtonRight.AsSpan(), StringComparison.Ordinal);
    }
}
