using Discord;
using Discord.WebSocket;
using LotusMusic.Core.Embeds;
using LotusMusic.Core.Local;
using LotusMusic.Core.Paging;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace LotusMusic.App.Modules.Paging;

internal class LocalFilesPager : PagerSupport
{
    private ILocalSource Source { get; }
    private DiscordSocketClient Client { get; }

    public LocalFilesPager(IServiceProvider provider) : base(provider, "local", 25)
    {
        Source = Provider.GetRequiredService<ILocalSource>();
        Client = Provider.GetRequiredService<DiscordSocketClient>();
    }

    public override Task<(Embed, MessageComponent)> PagerAsync(int pageNumber, 
        ButtonType buttonType)
    {
        IEnumerable<string> source = Source.ListAll();

        int itemCount = source.Count();

        MovePage(ref pageNumber, buttonType);

        int totalPages = TotalPages(itemCount);

        StringBuilder builder = new();

        IEnumerable<string> data = source.Skip(pageNumber * MaxPageCount).Take(MaxPageCount);

        int count = 1 + (pageNumber * MaxPageCount);

        foreach (var item in data)
        {
            builder.AppendLine($"**[{count++}]**: {item}");
        }

        var (Left, Right) = Pager.BuildPager(itemCount, pageNumber, MaxPageCount, PageName);


        var embed = new EmbedBuilder()
            .WithAuthor(Client.CurrentUser)
            .WithDescription(builder.ToString())
            .WithRandomColor()
            .WithCurrentTimestamp()
            .WithFooter($"{++pageNumber}/{totalPages}")
            .Build();

        var updatedComponent = new ComponentBuilder().WithButton(Left).WithButton(Right).Build();

        return Task.FromResult((embed, updatedComponent));
    }
}
