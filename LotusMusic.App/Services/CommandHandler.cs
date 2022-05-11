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
        Client.MessageReceived += Client_MessageReceived;
        Client.Ready += Client_Ready;

        await Service.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
    }

    public async ValueTask<string> GetPrefixAsync(IGuild guild)
    {
        var prefix = await Context.Prefixes.FindAsync(guild.Id);

        if (prefix is null)
        {
            return Configuration.GetSection("Client-Configuration")["Prefix"];
        }

        return prefix.Value ?? Configuration.GetSection("Client-Configuration")["Prefix"];
    }


    private async Task Client_MessageReceived(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message) return;
        if (message.Source is not MessageSource.User) return;

        int position = 0;

        string prefix = await GetPrefixAsync((message.Channel as SocketGuildChannel)!.Guild);

        if (!message.HasStringPrefix(prefix, ref position) && !message.HasMentionPrefix(Client.CurrentUser, ref position)) return;

        var context = new SocketCommandContext(Client, message);
        
        await Service.ExecuteAsync(context, position, Provider);
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

        Logger.LogInformation("Attemping to connect to Lavalink");
        if (!Node.IsConnected)
        {
            await Node.ConnectAsync();
        }
        Logger.LogInformation("After connect");
    }
}
