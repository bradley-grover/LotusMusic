using Discord.Commands;
using LotusMusic.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LotusMusic.App.Modules.Commands;

public class PrefixSetupCommands : CommandBase<PrefixSetupCommands>
{
    private IConfiguration Configuration { get; }
    private string DefaultPrefix => Configuration.GetSection("Client-Configuration")["Prefix"];
    
    public PrefixSetupCommands(IServiceProvider provider) : base(provider)
    {
        Configuration = provider.GetRequiredService<IConfiguration>();
    }

    [Command("prefix")]
    public async Task PrefixAsynx()
    {
        var prefix = await Database.Prefixes.FindAsync(Context.Guild.Id);

        if (prefix is null)
        {
            await ReplyAsync($"The prefix is the default prefix: {DefaultPrefix}");
            return;
        }

        await ReplyAsync($"The prefix is {prefix.Value}");
    }

    [Command("set_prefix")]
    public async Task SetPrefixAsync(string prefix)
    {
        if (prefix.Length > 3)
        {
            await ReplyAsync("The max length is 3 for a prefix");
            return;
        }

        var current = await Database.Prefixes.FindAsync(Context.Guild.Id);

        if (current is not null)
        {
            if (prefix == current.Value)
            {
                await ReplyAsync("The current prefix matches that value");
                return;
            }

            Database.Add(new Prefix()
            {
                Id = Context.Guild.Id,
                Value = prefix
            });

            await Database.SaveChangesAsync();

            await ReplyAsync($"Set prefix as: {prefix}");

            return;
        }

        Database.Add(new Prefix()
        {
            Id = Context.Guild.Id,
            Value = prefix,
        });

        await Database.SaveChangesAsync();

        await ReplyAsync($"Set prefix as {prefix}");
    }

    [Command("reset_prefix")]
    public async Task ResetPrefixAsync()
    {
        var prefix = await Database.Prefixes.FindAsync(Context.Guild.Id);

        if (prefix is null)
        {
            await Context.Channel.SendMessageAsync("The prefix is already default");
            return;
        }

        Database.Remove(prefix);

        await Database.SaveChangesAsync();

        await Context.Channel.SendMessageAsync($"Reset prefix to default: {DefaultPrefix}");
    }
}
