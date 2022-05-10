using Discord;
using Victoria;

namespace LotusMusic.Core.Music;

public class LavalinkAudio : IAudioPlayer
{
    private LavaNode Node { get; }
    public LavalinkAudio(LavaNode node)
    {
        Node = node;
    }

    public bool IsConnected(IGuild guild)
    {
        return Node.HasPlayer(guild);
    }

    public async Task<Embed> JoinAsync(IGuild guild, IVoiceState state, ITextChannel channel)
    {
        //if (IsConnected(guild))
        //{
        //}
        return default;
    }

    public Task<Embed> LeaveAsync(IGuild guild)
    {
        throw new NotImplementedException();
    }
}
