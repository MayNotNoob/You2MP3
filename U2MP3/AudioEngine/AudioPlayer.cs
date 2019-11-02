using NAudio;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using U2MP3.Models;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace U2MP3.AudioEngine
{
    public class AudioPlayer
    {
        private readonly object waveOutLock;
        private readonly SynchronizationContext syncContext;
        private IntPtr hWaveOut;
        private AudioBuffer[] buffers;
        private IWaveProvider waveStream;
        private volatile PlaybackState playbackState;
        private AutoResetEvent callbackEvent;
        private bool shouldFireEvent = true;
        private readonly YoutubeClient _youtubeClient;
        /// <summary>Indicates playback has stopped automatically</summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Gets or sets the desired latency in milliseconds
        /// Should be set before a call to Init
        /// </summary>
        public int DesiredLatency { get; set; }

        /// <summary>
        /// Gets or sets the number of buffers used
        /// Should be set before a call to Init
        /// </summary>
        public int NumberOfBuffers { get; set; }

        /// <summary>
        /// Gets or sets the device number
        /// Should be set before a call to Init
        /// This must be between -1 and <see>DeviceCount</see> - 1.
        /// -1 means stick to default device even default device is changed
        /// </summary>
        public int DeviceNumber { get; set; } = -1;

        public Music Music { get; set; }


        /// <summary>Opens a WaveOut device</summary>
        public AudioPlayer(YoutubeClient youtubeClient)
        {
            _youtubeClient = youtubeClient;
            this.syncContext = SynchronizationContext.Current;
            if (this.syncContext != null && (this.syncContext.GetType().Name == "LegacyAspNetSynchronizationContext" || this.syncContext.GetType().Name == "AspNetSynchronizationContext"))
                this.syncContext = (SynchronizationContext)null;
            this.DesiredLatency = 300;
            this.NumberOfBuffers = 2;
            this.waveOutLock = new object();
        }




        /// <summary>Start playing the audio from the WaveStream</summary>
        public async void Play()
        {
            Music.ImageState = ImageState.PLAYING;
            if (this.playbackState != PlaybackState.Paused)
                await RetrieveMusicInfo();
            if (this.buffers == null || this.waveStream == null)
                throw new InvalidOperationException("Must call Init first");
            if (this.playbackState == PlaybackState.Stopped)
            {
                shouldFireEvent = true;
                this.playbackState = PlaybackState.Playing;
                this.callbackEvent.Set();
                ThreadPool.QueueUserWorkItem((WaitCallback)(state => this.PlaybackThread()), (object)null);
            }
            else
            {
                if (this.playbackState != PlaybackState.Paused)
                    return;
                this.Resume();
                this.callbackEvent.Set();
            }
        }
        /// <summary>Initialises the WaveOut device</summary>
        /// <param name="waveProvider">WaveProvider to play</param>
        private void Init(IWaveProvider waveProvider)
        {
            if (this.playbackState != PlaybackState.Stopped)
                throw new InvalidOperationException("Can't re-initialize during playback");
            if (this.hWaveOut != IntPtr.Zero)
            {
                this.DisposeBuffers();
                this.CloseWaveOut();
            }
            this.callbackEvent = new AutoResetEvent(false);
            this.waveStream = waveProvider;
            int byteSize = waveProvider.WaveFormat.ConvertLatencyToByteSize((this.DesiredLatency + this.NumberOfBuffers - 1) / this.NumberOfBuffers);
            MmResult result;
            lock (this.waveOutLock)
                result = AudioInterop.waveOutOpenWindow(out this.hWaveOut, (IntPtr)this.DeviceNumber, this.waveStream.WaveFormat, this.callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, AudioInterop.WaveInOutOpenFlags.CallbackEvent);
            MmException.Try(result, "waveOutOpen");
            this.buffers = new AudioBuffer[this.NumberOfBuffers];
            this.playbackState = PlaybackState.Stopped;
            for (int index = 0; index < this.NumberOfBuffers; ++index)
                this.buffers[index] = new AudioBuffer(this.hWaveOut, byteSize, this.waveStream, this.waveOutLock);
        }


        private async Task RetrieveMusicInfo()
        {
            if (string.IsNullOrEmpty(Music.SourceUrl))
            {
                MediaStreamInfoSet infoSet = await _youtubeClient.GetVideoMediaStreamInfosAsync(Music.Id);
                AudioStreamInfo info = infoSet.Audio.WithHighestBitrate();
                Music.SourceUrl = info.Url;
            }
            var mf = new MediaFoundationReader(Music.SourceUrl);
            Init(mf);
        }
        private void PlaybackThread()
        {
            Exception e = (Exception)null;
            try
            {
                this.DoPlayback();
            }
            catch (Exception ex)
            {
                e = ex;
            }
            finally
            {
                this.playbackState = PlaybackState.Stopped;
                if (shouldFireEvent)
                    this.RaisePlaybackStoppedEvent(e);
            }
        }

        private void DoPlayback()
        {
            while (this.playbackState != PlaybackState.Stopped)
            {
                if (!this.callbackEvent.WaitOne(this.DesiredLatency))
                {
                    int playbackState = (int)this.playbackState;
                }
                if (this.playbackState == PlaybackState.Playing)
                {
                    int num = 0;
                    foreach (AudioBuffer buffer in this.buffers)
                    {
                        if (buffer.InQueue || buffer.OnDone())
                            ++num;
                    }
                    if (num == 0)
                    {
                        this.playbackState = PlaybackState.Stopped;
                        this.callbackEvent.Set();
                    }
                }
            }
        }

        /// <summary>Pause the audio</summary>
        public void Pause()
        {
            if (this.playbackState != PlaybackState.Playing)
                return;
            this.playbackState = PlaybackState.Paused;
            Music.ImageState = ImageState.PAUSE;
            MmResult result;
            lock (this.waveOutLock)
                result = AudioInterop.waveOutPause(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutPause");
        }

        /// <summary>Resume playing after a pause from the same position</summary>
        private void Resume()
        {
            if (this.playbackState != PlaybackState.Paused)
                return;
            MmResult result;
            lock (this.waveOutLock)
                result = AudioInterop.waveOutRestart(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutRestart");
            this.playbackState = PlaybackState.Playing;
        }

        public void StopWithoutEvent()
        {
            shouldFireEvent = false;
            Music.ImageState = ImageState.STOP;
            Stop();
        }

        /// <summary>Stop and reset the WaveOut device</summary>
        public void Stop()
        {
            if (this.playbackState == PlaybackState.Stopped)
                return;
            this.playbackState = PlaybackState.Stopped;
            MmResult result;
            lock (this.waveOutLock)
                result = AudioInterop.waveOutReset(this.hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutReset");
            this.callbackEvent.Set();
        }

        /// <summary>
        /// Gets the current position in bytes from the wave output device.
        /// (n.b. this is not the same thing as the position within your reader
        /// stream - it calls directly into waveOutGetPosition)
        /// </summary>
        /// <returns>Position in bytes</returns>
        public long GetPosition()
        {
            return AudioUtils.GetPositionBytes(this.hWaveOut, this.waveOutLock);
        }

        /// <summary>
        /// Gets a <see cref="T:NAudio.Wave.WaveFormat" /> instance indicating the format the hardware is using.
        /// </summary>
        public WaveFormat OutputWaveFormat
        {
            get
            {
                return this.waveStream.WaveFormat;
            }
        }

        /// <summary>Playback State</summary>
        public PlaybackState PlaybackState
        {
            get
            {
                return this.playbackState;
            }
        }

        /// <summary>Volume for this device 1.0 is full scale</summary>
        public float Volume
        {
            get
            {
                return AudioUtils.GetWaveOutVolume(this.hWaveOut, this.waveOutLock);
            }
            set
            {
                AudioUtils.SetWaveOutVolume(value, this.hWaveOut, this.waveOutLock);
            }
        }

        /// <summary>Closes this WaveOut device</summary>
        public void Dispose()
        {
            GC.SuppressFinalize((object)this);
            this.Dispose(true);
        }

        /// <summary>Closes the WaveOut device and disposes of buffers</summary>
        /// <param name="disposing">True if called from <see>Dispose</see></param>
        protected void Dispose(bool disposing)
        {
            this.Stop();
            if (disposing)
                this.DisposeBuffers();
            this.CloseWaveOut();
        }

        private void CloseWaveOut()
        {
            if (this.callbackEvent != null)
            {
                this.callbackEvent.Close();
                this.callbackEvent = (AutoResetEvent)null;
            }
            lock (this.waveOutLock)
            {
                if (!(this.hWaveOut != IntPtr.Zero))
                    return;
                int num = (int)AudioInterop.waveOutClose(this.hWaveOut);
                this.hWaveOut = IntPtr.Zero;
            }
        }

        private void DisposeBuffers()
        {
            if (this.buffers == null)
                return;
            foreach (AudioBuffer buffer in this.buffers)
                buffer.Dispose();
            this.buffers = (AudioBuffer[])null;
        }

        /// <summary>
        /// Finalizer. Only called when user forgets to call <see>Dispose</see>
        /// </summary>
        ~AudioPlayer()
        {
            this.Dispose(false);
        }

        private void RaisePlaybackStoppedEvent(Exception e)
        {
            EventHandler<StoppedEventArgs> handler = this.PlaybackStopped;
            if (handler == null)
                return;
            if (this.syncContext == null)
                handler((object)this, new StoppedEventArgs(e));
            else
                this.syncContext.Post((SendOrPostCallback)(state => handler((object)this, new StoppedEventArgs(e))), (object)null);
        }
    }
}