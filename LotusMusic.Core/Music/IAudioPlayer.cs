using Discord;
using Discord.WebSocket;

namespace LotusMusic.Core.Music;

public interface IAudioPlayer
{
    bool IsConnected(IGuild guild);
    Task<Embed> JoinAsync(IGuild guild, IVoiceState state, ITextChannel channel);
    Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query);
    Task<Embed> PlayLocalAsync(SocketGuild user, IGuild guild, string query);
    Task<Embed> SkipAsync(IGuild guild);
    Task<Embed> PauseAsync(IGuild guild);
    Task<Embed> ResumeAsync(IGuild guild);
    Task<Embed> LeaveAsync(IGuild guild);
    Task<Embed> ListAsync(IGuild guild);
}