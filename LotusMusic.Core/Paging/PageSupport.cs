using Discord;

namespace LotusMusic.Core.Paging;

public abstract class PagerSupport
{
    public PagerSupport(IServiceProvider provider, string pageName, int maxPageCount = 25)
    {
        Provider = provider;
        PageName = pageName;
        MaxPageCount = maxPageCount;
    }

    protected IServiceProvider Provider { get; }
    public string PageName { get; }
    protected int MaxPageCount { get; }

    public abstract Task<(Embed, MessageComponent)> PagerAsync(int pageNumber, ButtonType buttonType);

    protected int TotalPages(int totalItems)
    {
        return (int)Math.Ceiling((double)totalItems / MaxPageCount);
    }
    protected static void MovePage(ref int number, ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Left:
                number--;
                break;
            case ButtonType.Right:
                number++;
                break;
        }
    }
}