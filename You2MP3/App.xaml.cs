using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using You2MP3.ViewModels;
using You2MP3.YoutubeServices;

namespace You2MP3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider _serviceProvider;

        //public IConfiguration Configuration { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(s => new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBzUiFi3Wj-qU-_a0R2xH-fbTqpDEcQC14",
                ApplicationName = "You2MP3"
            }));
            serviceCollection.AddSingleton<SearchService>();
            serviceCollection.AddSingleton<MainWindowModel>();
            serviceCollection.AddSingleton<MainWindow>();
            _serviceProvider = serviceCollection.BuildServiceProvider();
            MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
