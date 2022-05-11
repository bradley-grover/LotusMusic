using Discord.Interactions;

namespace LotusMusic.Core.Music;

public enum PlayerSource
{
    [ChoiceDisplay("external")]
    External,
    [ChoiceDisplay("local")]
    Local
}