using Discord;
using System.Reflection;

namespace LotusMusic.Core.Paging;

public class PageResolver : IPageResolver
{
    private readonly Dictionary<string, PagerSupport> modules = new();

    public void AddModules(Assembly assembly, IServiceProvider provider)
    {
        if (assembly is null)
        {
            return;
        }

        IEnumerable<PagerSupport?> pagers = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PagerSupport)) && !t.IsAbstract)
            .Select(t => (PagerSupport?)Activator.CreateInstance(t, provider));
      
        foreach (var pager in pagers)
        {
            if (pager is null) continue;

            if (pager.PageName is null)
            {
                continue;
            }

            if (!modules.TryAdd(pager.PageName, pager))
            {
                throw new InvalidOperationException($"Duplicate pager name of {pager.PageName} was found");
            }
        }
    }

    public async Task<(Embed, MessageComponent)> ExecuteAsync(string pageName, int pageNumber, ButtonType buttonType)
    {
        return await modules[pageName].PagerAsync(pageNumber, buttonType);
    }
}
