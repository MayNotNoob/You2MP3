using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NAudio.Wave;
using U2MP3.AudioEngine;
using U2MP3.UServices;
using U2MP3.ViewModels;
using Unity;
using Unity.Injection;
using YoutubeExplode;

namespace U2MP3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            base.OnStartup(startupEventArgs);
            UnityContainer container = new UnityContainer();
            container.RegisterSingleton<YouTubeService>(new InjectionConstructor(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBzUiFi3Wj-qU-_a0R2xH-fbTqpDEcQC14",
                ApplicationName = "You2MP3"
            }));
            container.RegisterSingleton<YoutubeClient>();
            container.RegisterSingleton<UService>();
            container.RegisterSingleton<AudioPlayer>();
            container.RegisterSingleton<MainWindowModel>();
            container.RegisterSingleton<MainWindow>();
            MainWindow mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}
