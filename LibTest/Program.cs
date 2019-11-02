using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using NetCoreAudio;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
namespace LibTest
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var client = new YoutubeClient();
            var videoInfo = await client.GetVideoMediaStreamInfosAsync("jOxzAsnx9-0");

            AudioStreamInfo info = videoInfo.Audio[0];

            var buffer = new byte[128];
            Memory<byte> mem = new Memory<byte>();
            MemoryStream memoryStream = new MemoryStream();
            int len = 0;
            long lastPosition = 0;
            long present = 0;

            using (var mf = new SavingWaveProvider(new MediaFoundationReader(info.Url), $@"./audio.{info.Container.GetFileExtension()}"))
            using (var wo = new WaveOutEvent() {DesiredLatency = 30000})
            {
                //await mf.CopyToAsync(memoryStream);
                //mf.Seek(0, SeekOrigin.Begin);
                wo.Init(mf);
                //var x = mf.WaveFormat.ConvertLatencyToByteSize((wo.DesiredLatency + wo.NumberOfBuffers - 1) / wo.NumberOfBuffers);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(1000);
                }
            }
            //Console.WriteLine($"Song size: {info.Size}");
            //IProgress<double> progress = new Progress<double>(value =>
            //   {
            //       Console.WriteLine($"Finish : { Math.Round(value * 100, 2)} %");
            //   });
            //await client.DownloadMediaStreamAsync(info, @".\test.mp3", progress);

            //await using (var audioFile = new AudioFileReader(@".\test.mp3"))
            //using (var outputDevice = new WaveOutEvent())
            //{
            //    outputDevice.Init(audioFile);
            //    outputDevice.Play();
            //    while (outputDevice.PlaybackState == PlaybackState.Playing)
            //    {
            //       await Task.Delay(1000);
            //    }
            //}
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
