using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Victoria;

namespace LotusMusic.App.Services;

internal class CommandHandler : DiscordClientService
{
    private CommandService Service { get; }
    private ApplicationDbContext Context { get; }
    private IServiceProvider Provider { get; }
    private IConfiguration Configuration { get; }

    public CommandHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger,
        CommandService service, ApplicationDbContext context,
        IServiceProvider provider, IConfiguration configuration) 
        : base(client, logger)
    {
        Service = service;
        Context = context;
        Provider = provider;
        Configuration = configuration;
    }

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.Ready += Client_Ready;
        Client.MessageReceived += Client_MessageReceived;

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
        var node = Provider.GetService<LavaNode>();

        if (node is null) return;

        if (!node.IsConnected)
        {
            await node.ConnectAsync();
        }
    }
}
