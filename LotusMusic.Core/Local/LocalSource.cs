using Microsoft.Extensions.Configuration;
using File = TagLib.File;

namespace LotusMusic.Core.Local;

public class LocalSource : ILocalSource
{
    private bool _loaded;
    private string[]? Paths { get; set; }
    private List<(File file, string path)> Files { get; } = new();

    public IEnumerable<string> ListAll()
    {
        foreach (var (file, fileName) in Files)
        {
            if (file.Tag.Title is not null)
            {
                yield return file.Tag.Title;
                continue;
            }

            yield return Path.GetFileNameWithoutExtension(fileName);
        }
    }

    public void Load(IConfiguration config)
    {
        Paths = config.GetSection("Music").GetSection("Directories").Get<string[]>();

        GetAllFiles();

        _loaded = true;
    }
    public string? FindFile(string query)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (!_loaded)
        {
            throw new InvalidOperationException("You should call LoadAsync first before running find files");
        }

        query = query.ToLower().Trim();

        int index = Files.FindIndex(x => x.path.ToLower() == query);

        if (index != -1)
        {
            return Files[index].path;
        }

        if (FindByDirectMatch(query, out string? path))
        {
            return path;
        }


        return null;
    }
    private bool FindByDirectMatch(string query, out string? fullPath)
    {
        // iterate for direct title match

        foreach (var (file, path) in Files)
        {
            if (file.Tag.Title is null)
            {
                continue;
            }
            if (file.Tag.Title.ToLower().Trim() == query)
            {
                fullPath = path;
                return true;
            }
        }

        foreach (var (_, path) in Files)
        {
            if (Path.GetFileNameWithoutExtension(path).ToLower() == query)
            {
                fullPath = Path.GetFileNameWithoutExtension(path);
                return true;
            }
        }

        fullPath = null;
        return false;
    }

    private void GetAllFiles()
    {
        if (Paths is null)
        {
            return;
        }

        for (int i = 0; i < Paths.Length; i++)
        {
            DirectoryInfo info = new(Paths[i]);
            var files = info.GetFiles();
            
            for (int j = 0; j < files.Length; j++)
            {
                if (!(files[j].Extension == ".mp3")) continue;

                File mp3;
                try
                {
                    mp3 = File.Create(files[j].FullName);
                    Files.Add((mp3, files[j].FullName));
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
