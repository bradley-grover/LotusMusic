using Discord;
using Discord.Interactions;
using Humanizer;
using LotusMusic.Core.Embeds;
using System.Diagnostics;

namespace LotusMusic.App.Modules.SlashCommands;

public class InfoSlashCommands : InteractionBase<InfoSlashCommands>
{
    public InfoSlashCommands(IServiceProvider provider) : base(provider)
    {
    }

    [SlashCommand("ping", "checks if the client is awake")]
    public async Task PingAsync()
    {
        await RespondAsync($"Pong! API Latency is {DiscordClient.Latency}ms");
    }
    [SlashCommand("invite", "displays link to add the bot to one of your servers")]
    public async Task InviteAsync()
    {
        var embed = new EmbedBuilder()
            .WithDescription($"[Invite](https://discord.com/oauth2/authorize?client_id={DiscordClient.CurrentUser.Id}&scope=bot&permissions=8&scope=bot%20applications.commands)")
            .WithCurrentTimestamp()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithRandomColor()
            .Build();
        await RespondAsync(embed: embed);
    }

    [UserCommand("info")]
    [SlashCommand("info", "gets info on a user")]
    public async Task InfoAsync(IUser user)
    {
        user ??= Context.User;

        var builder = new EmbedBuilder()
            .WithAuthor(user)
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .AddField("User ID:", $"{user.Id}\n{user.Mention}", true)
            .AddField("Username:", $"{user.Username}#{user.Discriminator}", true)
            .AddField("User Status:", user.Status, true)
            .AddField("Account created:", $"{user.CreatedAt.Humanize()} ({user.CreatedAt.DateTime})")
            .WithRandomColor()
            .WithCurrentTimestamp();

        await RespondAsync(embed: builder.Build());
    }
}
