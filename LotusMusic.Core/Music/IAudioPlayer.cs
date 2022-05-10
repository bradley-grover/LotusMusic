using Discord;

namespace LotusMusic.Core.Music;

public interface IAudioPlayer
{
    bool IsConnected(IGuild guild);
    Task<Embed> JoinAsync(IGuild guild, IVoiceState state, ITextChannel channel);
    Task<Embed> LeaveAsync(IGuild guild);
}