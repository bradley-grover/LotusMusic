using Discord;
using System.Reflection;

namespace LotusMusic.Core.Paging;

public interface IPageResolver
{
    public void AddModules(Assembly assembly, IServiceProvider provider);
    public Task<(Embed, MessageComponent)> ExecuteAsync(string pageName, int pageNumber, ButtonType buttonType);
}
