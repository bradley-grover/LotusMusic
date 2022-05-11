using Discord;
using Discord.WebSocket;
using Victoria;

namespace LotusMusic.Core.Music;

public interface IAudioPlayer
{
    public LavaNode Node { get; }
    bool IsConnected(IGuild guild);
    Task<Embed> JoinAsync(IGuild guild, IVoiceState state, ITextChannel channel);
    Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query);
    Task<Embed> PlayMultipleTracksAsync(SocketGuildUser user, IGuild guild, IEnumerable<LavaTrack> track);
    Task<Embed> PlayLocalAsync(SocketGuildUser user, IGuild guild, string query);
    Task<Embed> SkipAsync(IGuild guild);
    Task<Embed> PauseAsync(IGuild guild);
    Task<Embed> ResumeAsync(IGuild guild);
    Task<Embed> LeaveAsync(IGuild guild);
    Task<Embed> ListAsync(IGuild guild);
}