using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace YtDownloader
{
    internal static class Program
    {
        public static IConfigurationRoot? Configuration;

        private static IHost? _host;
        public static IServiceProvider? ServiceProvider;

        [STAThread]
        static void Main()
        {
            _host = Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
                .ConfigureServices( (context,services) =>
                {
                    services
                        .AddSingleton<Downloader>()
                        .AddSingleton<MainForm>();
                }).Build();
            ServiceProvider = _host.Services;

            Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }
    }
}