using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace LotusMusic.Core.Bases;

public abstract class InteractionBase<T> : InteractionModuleBase<SocketInteractionContext>
    where T : InteractionBase<T>
{
    protected IServiceProvider Provider { get; }
    protected DiscordSocketClient DiscordClient { get; }

    public InteractionBase(IServiceProvider provider)
    {
        Provider = provider;
        DiscordClient = Provider.GetRequiredService<DiscordSocketClient>();
    }
}
