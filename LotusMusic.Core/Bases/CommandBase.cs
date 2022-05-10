using Discord.Commands;
using Discord.WebSocket;
using LotusMusic.Data;
using Microsoft.Extensions.DependencyInjection;

namespace LotusMusic.Core.Bases;

public abstract class CommandBase<T> : ModuleBase<SocketCommandContext>
    where T : CommandBase<T>
{
    protected IServiceProvider Provider { get; }
    protected ApplicationDbContext Database { get; }

    protected DiscordSocketClient DiscordClient { get; }
    
    public CommandBase(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));

        Provider = provider;

        Database = provider.GetRequiredService<ApplicationDbContext>();

        DiscordClient = provider.GetRequiredService<DiscordSocketClient>();
    }
}
