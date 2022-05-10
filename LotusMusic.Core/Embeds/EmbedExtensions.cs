using Discord;

namespace LotusMusic.Core.Embeds;

public static class EmbedExtensions
{
    public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
    {
        return builder.WithColor(new Color(GetRandomByte(), GetRandomByte(), GetRandomByte()));
    }

    private static byte GetRandomByte()
    {
        return (byte)Random.Shared.Next(255);
    }
}
