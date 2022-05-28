using Microsoft.Extensions.Configuration;

namespace LotusMusic.Core.Local;

public interface ILocalSource
{
    void Load(IConfiguration config);

    string? FindFile(string query);

    IEnumerable<string> ListAll();
}
