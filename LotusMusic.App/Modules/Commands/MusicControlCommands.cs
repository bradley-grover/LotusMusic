using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace LotusMusic.App.Modules.Commands;

public class MusicControlCommands : InteractionBase<MusicControlCommands>
{
    private IAudioPlayer Player { get; }
    public MusicControlCommands(IServiceProvider provider) : base(provider)
    {
        Player = provider.GetRequiredService<IAudioPlayer>();
    }

    [SlashCommand("join", "joins a voice channel")]
    public async Task JoinAsync()
    {
        await RespondAsync(embed: await Player.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!));
    }

    [SlashCommand("play", "plays the track based on the query provided")]
    public async Task PlayAsync([Remainder] string query, PlayerSource? source = null)
    {
        if (!Player.IsConnected(Context.Guild))
        {
            await Player.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!);
        }

        source ??= PlayerSource.External;

        switch (source)
        {
            case PlayerSource.External:
                await RespondAsync(embed: await Player.PlayAsync((Context.User as SocketGuildUser)!, Context.Guild, query));
                return;
            case PlayerSource.Local:
                await RespondAsync(embed: await Player.PlayLocalAsync((Context.User as SocketGuildUser)!, Context.Guild, query));
                return;
        }
    }

    [SlashCommand("pause", "pauses the currently playing track")]
    public async Task PauseAsync()
    {
        await RespondAsync(embed: await Player.PauseAsync(Context.Guild));
    }

    [SlashCommand("resume", "resumes a track if it is paused")]
    public async Task ResumeAsync()
    {
        await RespondAsync(embed: await Player.ResumeAsync(Context.Guild));
    }

    [SlashCommand("dc", "disconnects from the voice channel")]
    public async Task LeaveAsync()
    {
        await RespondAsync(embed: await Player.LeaveAsync(Context.Guild));
    }

    [SlashCommand("skip", "skips the current playing track")]
    public async Task SkipAsync()
    {
        await RespondAsync(embed: await Player.SkipAsync(Context.Guild));
    }

    [SlashCommand("list", "gets the current queue of tracks")]
    public async Task ListAsync()
    {
        await RespondAsync(embed: await Player.ListAsync(Context.Guild));
    }
}
