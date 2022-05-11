using Discord.Interactions;
using Discord.WebSocket;
using LotusMusic.Data;
using Microsoft.Extensions.DependencyInjection;

namespace LotusMusic.Core.Bases;

public abstract class InteractionBase<T> : InteractionModuleBase<SocketInteractionContext>
    where T : InteractionBase<T>
{
    protected IServiceProvider Provider { get; }
    protected DiscordSocketClient DiscordClient { get; }
    protected ApplicationDbContext Database { get; }

    public InteractionBase(IServiceProvider provider)
    {
        Provider = provider;
        DiscordClient = Provider.GetRequiredService<DiscordSocketClient>();
        Database = Provider.GetRequiredService<ApplicationDbContext>();
    }
}
