using Discord;
using LotusMusic.Core.Embeds;
using Victoria;

namespace LotusMusic.Core.Music;

public static class MusicHandler
{
    public static Embed CreateBasicEmbed(string title, string description, string? url = null)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithRandomColor()
            .WithThumbnailUrl(url)
            .WithCurrentTimestamp()
            .Build();
    }
    public static async ValueTask<Embed> FromPlayingTack(string channel, LavaTrack track)
    {
        return new EmbedBuilder()
            .WithCurrentTimestamp()
            .WithTitle($"Music :musical_note: - {channel}")
            .AddField("Artist", track.Author)
            .AddField("Now Playing", track.Title)
            .WithThumbnailUrl(await track.FetchArtworkAsync())
            .WithRandomColor()
            .Build();
    }
    public static Embed FromNotConnected(string channel)
    {
        return CreateBasicEmbed(channel, "Not connected to a voice channel");
    }
    public static Embed FromAlreadyConnected(string channel)
    {
        return CreateBasicEmbed(channel, "Already connected to a voice channel");
    }
}
