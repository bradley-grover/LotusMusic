using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace LotusMusic.Core.Music;

public partial class LavalinkAudio : IAudioPlayer
{
    private LavaNode Node { get; }
    private ILogger<IAudioPlayer> Logger { get; }

    public LavalinkAudio(LavaNode node, ILogger<IAudioPlayer> logger)
    {
        Node = node;
        Logger = logger;
        BindEvents();
    }

    public bool IsConnected(IGuild guild)
    {
        return Node.HasPlayer(guild);
    }

    public async Task<Embed> JoinAsync(IGuild guild, IVoiceState state, ITextChannel channel)
    {
        if (IsConnected(guild)) return MusicHandler.FromAlreadyConnected("Music - Join");
        if (state.VoiceChannel is null) return MusicHandler.FromNotConnected("Music - Join");

        try
        {
            await Node.JoinAsync(state.VoiceChannel, channel);
            return MusicHandler.CreateBasicEmbed("Music - Join", $"Joined {state.VoiceChannel.Name}");
        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Join", ex.Message);
        }
    }

    public async Task<Embed> LeaveAsync(IGuild guild)
    {
        if (!IsConnected(guild)) return MusicHandler.FromNotConnected("Leave");

        try
        {
            var player = Node.GetPlayer(guild);

            if (player.PlayerState is PlayerState.Playing)
            {
                await player.StopAsync();
            }

            await Node.LeaveAsync(player.VoiceChannel);

            return MusicHandler.CreateBasicEmbed("Music - Leave", "I have left the voice channel");
        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Leave", ex.Message);
        }
    }

    public Task<Embed> SkipAsync(IGuild guild)
    {
        throw new NotImplementedException();
    }

    public Task<Embed> PauseAsync(IGuild guild)
    {
        throw new NotImplementedException();
    }

    public Task<Embed> ResumeAsync(IGuild guild)
    {
        throw new NotImplementedException();
    }

    public Task<Embed> ListAsync(IGuild guild)
    {
        throw new NotImplementedException();
    }

    #region Play

    public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query)
    {
        if (user.VoiceChannel is null)
        {
            return MusicHandler.FromNotConnected("Music - Play");
        }

        if (!IsConnected(guild))
        {
            return MusicHandler.FromNotConnected("Music - Play");
        }

        try
        {
            var player = Node.GetPlayer(guild);

            LavaTrack? track;

            var search = Uri.IsWellFormedUriString(query, UriKind.Absolute)
                ? await Node.SearchAsync(SearchType.YouTube, query)
                : await Node.SearchYouTubeAsync(query);

            if (search.Status == SearchStatus.NoMatches)
            {

            }

            track = search.Tracks.FirstOrDefault();

            if (track is null)
            {
                return MusicHandler.CreateBasicEmbed("Music - Error", "Could not get track");
            }

            if ((player.Track != null && player.PlayerState is PlayerState.Playing) || player.PlayerState is PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                return MusicHandler.CreateBasicEmbed("Music", $"{track?.Title} has been added to queue.");
            }

            await player.PlayAsync(track);

            return await MusicHandler.FromPlayingTack("Play", track);


        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Error", ex.Message);
        }
    }

    public async Task<Embed> PlayLocalAsync(SocketGuild user, IGuild guild, string query)
    {
        return MusicHandler.CreateBasicEmbed("Music - Play", "Feature is not supported yet");
    }

    #endregion
}
