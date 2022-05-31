using Discord;

namespace LotusMusic.Core.Paging;

public static class Pager
{
    internal const string ButtonLeft = "left-button-";
    internal const string ButtonRight = "right-button-";

    public static (ButtonBuilder Left, ButtonBuilder Right) BuildPager(int totalElements, int position, int maxPerPage, string type)
    {
        var leftButton = new ButtonBuilder()
            .WithLabel("<-")
            .WithCustomId($"{ButtonLeft}{type}/{position}")
            .WithStyle(ButtonStyle.Primary);

        if (position == 0)
        {
            leftButton.WithDisabled(true);
        }

        var rightButton = new ButtonBuilder()
            .WithLabel("->")
            .WithCustomId($"{ButtonRight}{type}/{position}")
            .WithStyle(ButtonStyle.Primary);

        double totalPages = Math.Ceiling((double)totalElements / maxPerPage);

        if (position >= (totalPages - 1))
        {
            rightButton.WithDisabled(true);
        }

        return (leftButton, rightButton);
    }

    public static string GetCaller(ReadOnlySpan<char> value)
    {
        int startAt = value.LastIndexOf("-") + 1;
        int endAt = value.LastIndexOf("/");

        return value[startAt..endAt].ToString();

    }
    public static int GetPageIndex(ReadOnlySpan<char> customId)
    {
        var value = customId.LastIndexOf("/");

        return int.Parse(customId[(value + 1)..]);
    }

    public static ButtonType GetButtonType(ReadOnlySpan<char> value)
    {
        if (value.Contains(ButtonLeft.AsSpan(), StringComparison.Ordinal))
        {
            return ButtonType.Left;
        }
        else
        {
            return ButtonType.Right;
        }
    }

    public static (string Caller, int PageNumber, ButtonType ButtonClicked) GetPageInfo(ReadOnlySpan<char> value)
    {
        return (GetCaller(value), GetPageIndex(value), GetButtonType(value));
    }

    public static bool IsPagedEmbed(ReadOnlySpan<char> customId)
    {
        return customId.Contains(ButtonLeft.AsSpan(), StringComparison.Ordinal)
            || customId.Contains(ButtonRight.AsSpan(), StringComparison.Ordinal);
    }
}
