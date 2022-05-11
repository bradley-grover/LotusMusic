using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace LotusMusic.App.Modules.Commands;

public class MusicControlCommands : CommandBase<MusicControlCommands>
{
    private IAudioPlayer Player { get; }
    public MusicControlCommands(IServiceProvider provider) : base(provider)
    {
        Player = provider.GetRequiredService<IAudioPlayer>();
    }

    [Command("join")]
    public async Task JoinAsync()
    {
        await ReplyAsync(embed: await Player.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!));
    }

    [Command("play")]
    public async Task PlayAsync([Remainder] string query)
    {
        if (!Player.IsConnected(Context.Guild))
        {
            await Player.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!);
        }
        await ReplyAsync(embed: await Player.PlayAsync((Context.User as SocketGuildUser)!, Context.Guild, query));
    }

    [Command("leave")]
    [Alias("dc", "disconnect")]
    public async Task LeaveAsync()
    {
        await ReplyAsync(embed: await Player.LeaveAsync(Context.Guild));
    }
}
