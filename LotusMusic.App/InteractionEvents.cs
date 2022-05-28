using Discord;

namespace LotusMusic.App;

internal static class InteractionEvents
{
    internal const string ButtonLeft = "left-button-";
    internal const string ButtonRight = "right-button-";

    internal const string LocalEmbed = "local-";

    internal static (ButtonBuilder Left, ButtonBuilder Right) BuildPager(int totalElements, int position, int maxPerPage, string type)
    {
        var leftButton = new ButtonBuilder()
            .WithLabel("<-")
            .WithCustomId($"{ButtonLeft}{type}{position}")
            .WithStyle(ButtonStyle.Primary);

        if (position == 0)
        {
            leftButton.WithDisabled(true);
        }

        var rightButton = new ButtonBuilder()
            .WithLabel("->")
            .WithCustomId($"{ButtonRight}{type}{position}")
            .WithStyle(ButtonStyle.Primary);

        if (totalElements <= maxPerPage)
        {
            rightButton.WithDisabled(true);
        }

        return (leftButton, rightButton);
    }
}
