using Discord;
using LotusMusic.Core.Embeds;
using System.Text.RegularExpressions;
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

    //public static string CreateProgressBar(LavaTrack track)
    //{
    //    int size = 25;

    //    char line = '▬';

        

    //    string slider = "🔘";

    //    if (track is null)
    //    {;
    //        return $"{slider}{new string(line, size-1)}";
    //    }

    //    TimeSpan current = track.Duration != TimeSpan.FromSeconds(0) ? track.Position : track.Duration;

    //    var total = track.Duration;

    //    string value = Regex.Replace(new string(line, (int)Math.Round(size / 2 * (current / total))), ".$", slider);

    //    var bar = current > total ?
    //        (new string(line, size / 2 * 2), (current / total) * 100) 
    //        :
    //        (value, current / total);

    //    if (bar.Item2.ToString().Contains(slider))
    //    {
    //        return $"{slider}{new string(line, size-1)}";
    //    }

    //    return $"{bar.Item1}";
    //}

    public static string CreateProgressBar(LavaTrack track)
    {
        int size = 25;
        char line = '▬';
        string slider = "🔘";

        if (track.Position >= track.Duration)
        {
            return new string(line, size + 2);
        }
        else
        {
            var percentage = track.Position / track.Duration;

            var progress = Math.Round(size * percentage);

            var empty = size - progress;

            var progressText = Regex.Replace(new string(line, (int)progress), ".$", slider);

            var emptyProgressText = new string(line, (int)empty);

            var bar = progressText + emptyProgressText;

            return bar;
        }
    }
}
