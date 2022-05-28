using Microsoft.Extensions.Logging;
using System.Text;
using Victoria.EventArgs;
using Humanizer;
using Victoria;
using Discord.WebSocket;

namespace LotusMusic.Core.Music;

public partial class LavalinkAudio
{
    private readonly StringBuilder statsBuilder = new();
    private void BindEvents()
    {
        Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        Node.OnTrackEnded += Node_OnTrackEnded;
        Node.OnStatsReceived += Node_OnStatsReceived;
        Node.OnTrackException += Node_OnTrackException;
    }

    private async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (user.Id == Client.CurrentUser.Id)
        {
            await HandleForClientDisconnectAsync(before, after);
            return;
        }

        if (before.VoiceChannel is null)
        {
            return;
        }

        if (after.VoiceChannel is not null)
        {
            return;
        }

        if (!before.VoiceChannel.Users.Any(x => x.Id == Client.CurrentUser.Id))
        {
            return;
        }

        if (before.VoiceChannel.Users.Count > 1)
        {
            return;
        }

        await LeaveAsync(before.VoiceChannel.Guild);
    }

    private async Task HandleForClientDisconnectAsync(SocketVoiceState before, SocketVoiceState after)
    {
        if (before.VoiceChannel is null) return;
        if (after.VoiceChannel is null)
        {
            await LeaveAsync(before.VoiceChannel.Guild);
        }
    }

    private async Task Node_OnTrackEnded(TrackEndedEventArgs args)
    {
        var player = args.Player;

        if (player.Queue is null)
        {
            return;
        }

        if (!player.Queue.TryDequeue(out var queueable))
        {
            await player.TextChannel.SendMessageAsync("Finished music queue");
            // _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(10));
            return;
        }


        if (queueable is not LavaTrack track)
        {
            await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        }

        await args.Player.PlayAsync(track);

        await args.Player.TextChannel.SendMessageAsync(embed: await MusicHandler.FromPlayingTack("Autoplay", track));
    }


    private Task Node_OnStatsReceived(StatsEventArgs arg)
    {
        statsBuilder.Clear();
        statsBuilder.AppendLine($"Current Player Count: {arg.Players}");
        statsBuilder.AppendLine($"Lavalink Memory Used: {Math.Round((float)arg.Memory.Allocated / 0x400 / 0x400, 2)}MiB");
        statsBuilder.AppendLine($"Lavalink Uptime: {arg.Uptime.Humanize(3, minUnit: Humanizer.Localisation.TimeUnit.Second)}");
        statsBuilder.AppendLine($"Currently Playing: {arg.PlayingPlayers}/{arg.Players}");

        Logger.LogInformation("{builder}", statsBuilder.ToString());

        return Task.CompletedTask;
    }

    private async Task Node_OnTrackException(TrackExceptionEventArgs arg)
    {
        Logger.LogError("{err}", arg.Exception);
        arg.Player.Queue.Enqueue(arg.Track);
        await arg.Player.TextChannel.SendMessageAsync($"{arg.Track.Title} has been re-added to the queue after an error");
    }
}
