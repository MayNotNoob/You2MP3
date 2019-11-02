using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace LibFramTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var client = new YoutubeClient();
            //var videoInfo = await client.GetVideoMediaStreamInfosAsync("jOxzAsnx9-0");

            //AudioStreamInfo info = videoInfo.Audio[0];
            //Console.WriteLine($"Song size: {info.Size}");
            //IProgress<double> progress = new Progress<double>(value =>
            //   {
            //       Console.WriteLine($"Finish : { Math.Round(value * 100, 2)} %");
            //   });
            //await client.DownloadMediaStreamAsync(info, @".\test.mp3", progress);
            //WindowsMediaPlayer wplayer = new WindowsMediaPlayer();

            //wplayer.URL = "My MP3 file.mp3";
            //wplayer.controls.play();
            Console.ReadLine();
        }

    }
}
