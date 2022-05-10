using Microsoft.Extensions.Hosting;

namespace LotusMusic.App;

public class Program
{
    public static async Task Main()
    {
        await Startup.CreateHostBuilder().Build().RunAsync();
    }
}
