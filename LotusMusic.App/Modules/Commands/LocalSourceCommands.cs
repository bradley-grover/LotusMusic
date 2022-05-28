using Discord;
using Discord.Interactions;
using LotusMusic.Core.Embeds;
using LotusMusic.Core.Local;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace LotusMusic.App.Modules.Commands;

public class LocalSourceCommands : InteractionBase<LocalSourceCommands>
{
    private ILocalSource Source { get; }
    public LocalSourceCommands(IServiceProvider provider) : base(provider)
    {
        Source = Provider.GetRequiredService<ILocalSource>();
    }

    [SlashCommand("list_locals", "lists all the local files for the local source")]
    public async Task ListLocalsAsync()
    {
        int pageCount = 25;

        var originalMemory = Source.ListAll();

        int totalCount = originalMemory.Count();

        double totalPages = Math.Ceiling((double)totalCount / pageCount);

        IEnumerable<string> source = originalMemory.Take(pageCount);

        StringBuilder builder = new();

        int count = 1;

        foreach (var item in source)
        {
            builder.AppendLine($"**{count++}** - **{item}**");
        }

        var (Left, Right) = InteractionEvents.BuildPager(totalCount, 0, pageCount, InteractionEvents.LocalEmbed);

        var embed = new EmbedBuilder()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithDescription(builder.ToString())
            .WithRandomColor()
            .WithCurrentTimestamp()
            .WithFooter($"1/{totalPages}")
            .Build();

        await RespondAsync(embed: embed, components: new ComponentBuilder().WithButton(Left).WithButton(Right).Build());
    }
}
