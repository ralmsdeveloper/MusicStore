using Microsoft.AspNetCore.Hosting;

namespace MusicStore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultConfiguration(args)
                .UseIIS()
                .UseStartup("MusicStore")
                .Build();

            host.Run();
        }
    }
}
