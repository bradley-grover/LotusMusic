using Discord;
using Discord.Interactions;
using LotusMusic.Core.Embeds;
using LotusMusic.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Victoria;
using Victoria.Responses.Search;

namespace LotusMusic.App.Modules.Commands;

public class PlaylistCommands : InteractionBase<PlaylistCommands>
{
    private IAudioPlayer Player { get; }
    public PlaylistCommands(IServiceProvider provider) : base(provider)
    {
        Player = provider.GetRequiredService<IAudioPlayer>();
    }


    [SlashCommand("create_playlist", "creates a empty playlist with the specified name")]
    public async Task CreatePlaylistAsync(string playlistName)
    {
        var user = await Database.MusicUsers
            .Include(x => x.Playlists)
            .Where(x => x.Id == Context.User.Id)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            user = new MusicUser()
            {
                Id = Context.User.Id,
            };

            Database.MusicUsers.Add(user);

            await Database.SaveChangesAsync();
        }

        Guid id = Guid.NewGuid();

        user.Playlists.Add(new Playlist()
        {
            Id = id,
            PlaylistName = playlistName
        });

        await Database.SaveChangesAsync();

        await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlist - Create", $"{playlistName} created with an ID of: {id}"));
    }

    [SlashCommand("add_to_playlist", "adds a song to the playlist")]
    public async Task AddToPlaylistAsync(string name, string query, PlayerSource? source = null)
    {
        var user = await Database.MusicUsers
            .Include(x => x.Playlists)
            .ThenInclude(x => x.Tracks)
            .Where(x => x.Id == Context.User.Id)
            .FirstOrDefaultAsync();

        if (user is null || !user.Playlists.Any())
        {
            await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", "You have no playlists, use 'create_playlist' to create a playlist"));
            return;
        }

        if (!user.Playlists.Any(x => x.PlaylistName.ToLower() == name.ToLower()))
        {
            await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", "No playlists named that use 'playlists' to see created playlists"));
            return;
        }

        source ??= PlayerSource.External;

        var index = user.Playlists.FindIndex(x => x.PlaylistName.ToLower() == name.ToLower());

        var response = source switch
        {
            PlayerSource.Local => await Player.Node.SearchAsync(SearchType.Direct, query),
            _ => await Player.Node.SearchYouTubeAsync(query),
        };

        switch (response.Status)
        {
            case SearchStatus.NoMatches:
                await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", "No matches found for query"));
                return;
            case SearchStatus.LoadFailed:
                await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", $"{response.Exception}"));
                break;
            case SearchStatus.SearchResult:
                var track = response.Tracks.FirstOrDefault();

                if (track is null)
                {
                    await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", "No matches found for query"));
                    return;
                }

                user.Playlists[index].Tracks.Add(track);

                Database.Update(user);

                await Database.SaveChangesAsync();

                await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", $"Added {track.Url} to the playlist"));

                return;
            case SearchStatus.PlaylistLoaded:
                user.Playlists[index].Tracks.AddRange(response.Tracks);

                Database.Update(user);
                await Database.SaveChangesAsync();

                await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", $"Added {response.Tracks.Count} tracks to the playlist"));

                return;
        }
    }

    [SlashCommand("playlists", "view all your playlists")]
    public async Task ViewPlaylistsAsync()
    {
        var user = await Database.MusicUsers
            .Include(x => x.Playlists)
            .ThenInclude(x => x.Tracks)
            .Where(x => x.Id == Context.User.Id)
            .FirstOrDefaultAsync();

        if (user is null || !user.Playlists.Any())
        {
            await RespondAsync(embed: MusicHandler.CreateBasicEmbed("Playlists", "You have no playlists, use 'create_playlist' to create a playlist"));
            return;
        }

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(Context.User)
            .WithTitle("Playlists")
            .WithCurrentTimestamp()
            .WithRandomColor();

        StringBuilder builder = new();

        var i = 1;

        foreach (var playlist in user.Playlists)
        {
            builder.AppendLine($"**{i++}**: **{playlist.PlaylistName}** - **{playlist.Tracks.Count}** tracks\n");
        }

        await RespondAsync(embed: embedBuilder.WithDescription(builder.ToString()).Build());
    }
}
