using Discord;
using Discord.WebSocket;
using Humanizer;
using LotusMusic.Core.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace LotusMusic.Core.Music;

public partial class LavalinkAudio : IAudioPlayer
{
    public LavaNode Node { get; }
    private ILogger<IAudioPlayer> Logger { get; }
    public DiscordSocketClient Client { get; }
    private ILocalSource LocalSource { get; }

    public LavalinkAudio(LavaNode node, ILogger<IAudioPlayer> logger, DiscordSocketClient client,
        ILocalSource localSource, IConfiguration configuration)
    {
        Node = node;
        Logger = logger;
        Client = client;
        LocalSource = localSource;
        BindEvents();
        LocalSource.Load(configuration);
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

    public async Task<Embed> SkipAsync(IGuild guild)
    {
        if (!IsConnected(guild)) return MusicHandler.FromNotConnected("Skip");

        try
        {
            var player = Node.GetPlayer(guild);

            if (player.Queue.Count < 1)
            {
                return MusicHandler.CreateBasicEmbed("Music - Skip", "Unable to skip track as there are no other tracks available");
            }
            var currentTrack = player.Track;

            await player.SkipAsync();

            return MusicHandler.CreateBasicEmbed("Music - Skip", $"Skipped **{currentTrack.Author}** - **{currentTrack.Title}**", await currentTrack.FetchArtworkAsync());
        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Skip", ex.Message);
        }
    }

    public async Task<Embed> PauseAsync(IGuild guild)
    {
        if (!IsConnected(guild)) return MusicHandler.FromNotConnected("Pause");

        try
        {
            var player = Node.GetPlayer(guild);

            if (player.PlayerState is not PlayerState.Playing)
            {
                await player.PauseAsync();
                return MusicHandler.CreateBasicEmbed("Music - Pause", "There is nothing to pause");
            }

            await player.PauseAsync();

            var embed = MusicHandler.CreateBasicEmbed($"Music - Pause", $"**Paused:** {player.Track.Title}", await player.Track.FetchArtworkAsync());

            return embed.ToEmbedBuilder()
                .AddField("Time In: ", $"{player.Track.Position.ToString(@"hh\:mm\:ss")} / {player.Track.Duration.ToString(@"hh\:mm\:ss")}\n\n{MusicHandler.CreateProgressBar(player.Track)}")
                .Build();
        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Pause", ex.Message);
        }
    }

    public async Task<Embed> ResumeAsync(IGuild guild)
    {
        if (!IsConnected(guild)) return MusicHandler.FromNotConnected("Resume");
        try
        {
            var player = Node.GetPlayer(guild);


            if (player.PlayerState is PlayerState.Playing)
            {
                return MusicHandler.CreateBasicEmbed("Music - Resume", $"Player is currently playing", await player.Track.FetchArtworkAsync());
            }

            if (player.PlayerState is PlayerState.Paused)
            {
                await player.ResumeAsync();
            }


            var embed = MusicHandler.CreateBasicEmbed("Music - Resume", $"**Resumed:** {player.Track.Title}", await player.Track.FetchArtworkAsync());

            return embed.ToEmbedBuilder()
                .AddField("Time In: ", $"{player.Track.Position.ToString(@"hh\:mm\:ss")} / {player.Track.Duration.ToString(@"hh\:mm\:ss")}\n\n{MusicHandler.CreateProgressBar(player.Track)}")
                .Build();
        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - Resume", ex.Message);
        }
    }

    #region List

    public async Task<Embed> ListAsync(IGuild guild)
    {
        return await InternalListAsync(guild);
    }

    private async ValueTask<Embed> InternalListAsync(IGuild guild)
    {
        if (!IsConnected(guild)) return MusicHandler.FromNotConnected("Queue");
        try
        {
            var descriptionBuilder = new StringBuilder();

            LavaPlayer player = Node.GetPlayer(guild);


            if (player.PlayerState is PlayerState.Playing)
            {
              
                if (player.Queue.Count < 1 && player.Track != null)
                {
                    return MusicHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: {Format.Url(player.Track.Title, player.Track.Url)} \n\n" +
                        $"{player?.Track?.Position.ToString(@"hh\:mm\:ss")} / {player?.Track?.Duration.ToString(@"hh\:mm\:ss")}\n{MusicHandler.CreateProgressBar(player?.Track!)}\n\n" +
                        "Nothing else is queued", await player!.Track.FetchArtworkAsync());
                }

                var trackNum = 2;

                foreach (LavaTrack track in player.Queue)
                {
                    descriptionBuilder.Append($"**[{trackNum++}]**: {track.Author ?? "Unknown"} - {track.Title}\n");
                }

                return MusicHandler.CreateBasicEmbed("Music Playlist", $"Now Playing:{Format.Url(player.Track!.Title, player.Track.Url)}\n\n" +
                    $"{player?.Track?.Position.ToString(@"hh\:mm\:ss")} / {player?.Track?.Duration.ToString(@"hh\:mm\:ss")}\n{MusicHandler.CreateProgressBar(player?.Track!)}\n\n" +
                    descriptionBuilder.ToString(), await player!.Track.FetchArtworkAsync());

            }

            return MusicHandler.CreateBasicEmbed("Music - List", "Player doesn't seem to be playing anything right now.");

        }
        catch (Exception ex)
        {
            return MusicHandler.CreateBasicEmbed("Music - List", ex.Message);
        }
    }
    #endregion

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
                return MusicHandler.CreateBasicEmbed("Music - Play", $"Could not find track for query: **{query}**");
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

    public async Task<Embed> PlayLocalAsync(SocketGuildUser user, IGuild guild, string query)
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

            string? trackPath = LocalSource.FindFile(query);

            if (trackPath is null)
            {
                return MusicHandler.CreateBasicEmbed("Music - Play", "Could not find file locally");
            }

            var search = await Node.SearchAsync(SearchType.Direct, trackPath);

            if (search.Status == SearchStatus.NoMatches)
            {
                return MusicHandler.CreateBasicEmbed("Music - Play", $"Could not find track for query: **{query}**");
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

    public Task<Embed> PlayMultipleTracksAsync(SocketGuildUser user, IGuild guild, IEnumerable<LavaTrack> track)
    {
        throw new NotImplementedException();
    }

    #endregion
}
