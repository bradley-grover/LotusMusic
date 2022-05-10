using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LotusMusic.Core;
using LotusMusic.Core.Embeds;
using Microsoft.Extensions.Logging;

namespace LotusMusic.App.Modules.Commands;

public class GeneralCommands : CommandBase<GeneralCommands>
{
    public GeneralCommands(IServiceProvider provider) : base(provider)
    {
    }

    [Command("invite")]
    [Summary("Provided a bot invite link")]
    public async Task InviteAsync()
    {
        var embed = new EmbedBuilder()
            .WithDescription($"[Invite](https://discord.com/oauth2/authorize?client_id={DiscordClient.CurrentUser.Id}&scope=bot&permissions=8)")
            .WithCurrentTimestamp()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithRandomColor()
            .Build();
        await Context.Channel.SendMessageAsync(embed: embed);
    }
}
