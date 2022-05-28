using Discord;
using Discord.WebSocket;
using Humanizer;
using LotusMusic.Core.Embeds;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace LotusMusic.App.Services;

public class StatusHandler : IHostedService
{
    private ulong AlertChannel { get; set; }

    public DiscordSocketClient Client { get; }
    public IConfiguration Config { get; }
    

    public StatusHandler(DiscordSocketClient client, IConfiguration config)
    {
        Client = client;
        Config = config;

        AlertChannel = Config.GetSection("Client-Configuration").GetValue<ulong>("Alert-Channel");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Client.SetGameAsync($"music for {Client.Guilds.Count} servers");
        await Client.SetStatusAsync(UserStatus.DoNotDisturb);

        Client.GuildAvailable += Client_GuildAvailable;
    }

    private async Task Client_GuildAvailable(SocketGuild arg)
    {
        await Client.SetGameAsync($"music for {Client.Guilds.Count} servers");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var channel = await Client.GetChannelAsync(AlertChannel);

        (channel as ISocketMessageChannel)?.SendMessageAsync(embed: new EmbedBuilder()
            .WithAuthor(Client.CurrentUser)
            .WithTitle("Update")
            .WithDescription($"Shutting down process of client {Client.CurrentUser.Mention}")
            .AddField("Uptime", (DateTime.Now-Process.GetCurrentProcess().StartTime).Humanize())
            .AddField("Environment",
#if DEBUG
            "Debug"
#else
            "Release"
#endif
            )
            .WithThumbnailUrl(Client.CurrentUser.GetAvatarUrl() ?? Client.CurrentUser.GetDefaultAvatarUrl())
            .WithCurrentTimestamp()
            .WithRandomColor()
            .Build()
            );
    }
}
