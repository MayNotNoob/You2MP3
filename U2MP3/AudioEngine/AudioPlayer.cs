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
        private readonly object _waveOutLock;
        private readonly SynchronizationContext _syncContext;
        private IntPtr _hWaveOut;
        private AudioBuffer[] _buffers;
        private IWaveProvider _waveStream;
        private volatile PlaybackState _playbackState;
        private AutoResetEvent _callbackEvent;
        private bool _shouldFireEvent = true;
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
            this._syncContext = SynchronizationContext.Current;
            if (this._syncContext != null && (this._syncContext.GetType().Name == "LegacyAspNetSynchronizationContext" || this._syncContext.GetType().Name == "AspNetSynchronizationContext"))
                this._syncContext = (SynchronizationContext)null;
            this.DesiredLatency = 300;
            this.NumberOfBuffers = 2;
            this._waveOutLock = new object();
        }




        /// <summary>Start playing the audio from the WaveStream</summary>
        public async void Play()
        {
            Music.ImageState = ImageState.PLAYING;
            if (this._playbackState != PlaybackState.Paused)
                await RetrieveMusicInfo();
            if (this._buffers == null || this._waveStream == null)
                throw new InvalidOperationException("Must call Init first");
            if (this._playbackState == PlaybackState.Stopped)
            {
                _shouldFireEvent = true;
                this._playbackState = PlaybackState.Playing;
                this._callbackEvent.Set();
                ThreadPool.QueueUserWorkItem((WaitCallback)(state => this.PlaybackThread()), (object)null);
            }
            else
            {
                if (this._playbackState != PlaybackState.Paused)
                    return;
                this.Resume();
                this._callbackEvent.Set();
            }
        }
        /// <summary>Initialises the WaveOut device</summary>
        /// <param name="waveProvider">WaveProvider to play</param>
        private void Init(IWaveProvider waveProvider)
        {
            if (this._playbackState != PlaybackState.Stopped)
                throw new InvalidOperationException("Can't re-initialize during playback");
            if (this._hWaveOut != IntPtr.Zero)
            {
                this.DisposeBuffers();
                this.CloseWaveOut();
            }
            this._callbackEvent = new AutoResetEvent(false);
            this._waveStream = waveProvider;
            int byteSize = waveProvider.WaveFormat.ConvertLatencyToByteSize((this.DesiredLatency + this.NumberOfBuffers - 1) / this.NumberOfBuffers);
            MmResult result;
            lock (this._waveOutLock)
                result = AudioInterop.waveOutOpenWindow(out this._hWaveOut, (IntPtr)this.DeviceNumber, this._waveStream.WaveFormat, this._callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, AudioInterop.WaveInOutOpenFlags.CallbackEvent);
            MmException.Try(result, "waveOutOpen");
            this._buffers = new AudioBuffer[this.NumberOfBuffers];
            this._playbackState = PlaybackState.Stopped;
            for (int index = 0; index < this.NumberOfBuffers; ++index)
                this._buffers[index] = new AudioBuffer(this._hWaveOut, byteSize, this._waveStream, this._waveOutLock);
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
                this._playbackState = PlaybackState.Stopped;
                if (_shouldFireEvent)
                    this.RaisePlaybackStoppedEvent(e);
            }
        }

        private void DoPlayback()
        {
            while (this._playbackState != PlaybackState.Stopped)
            {
                if (!this._callbackEvent.WaitOne(this.DesiredLatency))
                {
                    int playbackState = (int)this._playbackState;
                }
                if (this._playbackState == PlaybackState.Playing)
                {
                    int num = 0;
                    foreach (AudioBuffer buffer in this._buffers)
                    {
                        if (buffer.InQueue || buffer.OnDone())
                            ++num;
                    }
                    if (num == 0)
                    {
                        this._playbackState = PlaybackState.Stopped;
                        this._callbackEvent.Set();
                    }
                }
            }
        }

        /// <summary>Pause the audio</summary>
        public void Pause()
        {
            if (this._playbackState != PlaybackState.Playing)
                return;
            this._playbackState = PlaybackState.Paused;
            Music.ImageState = ImageState.PAUSE;
            MmResult result;
            lock (this._waveOutLock)
                result = AudioInterop.waveOutPause(this._hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutPause");
        }

        /// <summary>Resume playing after a pause from the same position</summary>
        private void Resume()
        {
            if (this._playbackState != PlaybackState.Paused)
                return;
            MmResult result;
            lock (this._waveOutLock)
                result = AudioInterop.waveOutRestart(this._hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutRestart");
            this._playbackState = PlaybackState.Playing;
        }

        public void StopWithoutEvent()
        {
            _shouldFireEvent = false;
            Music.ImageState = ImageState.STOP;
            Stop();
        }

        /// <summary>Stop and reset the WaveOut device</summary>
        public void Stop()
        {
            if (this._playbackState == PlaybackState.Stopped)
                return;
            this._playbackState = PlaybackState.Stopped;
            MmResult result;
            lock (this._waveOutLock)
                result = AudioInterop.waveOutReset(this._hWaveOut);
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutReset");
            this._callbackEvent.Set();
        }

        /// <summary>
        /// Gets the current position in bytes from the wave output device.
        /// (n.b. this is not the same thing as the position within your reader
        /// stream - it calls directly into waveOutGetPosition)
        /// </summary>
        /// <returns>Position in bytes</returns>
        public long GetPosition()
        {
            return AudioUtils.GetPositionBytes(this._hWaveOut, this._waveOutLock);
        }

        /// <summary>
        /// Gets a <see cref="T:NAudio.Wave.WaveFormat" /> instance indicating the format the hardware is using.
        /// </summary>
        public WaveFormat OutputWaveFormat
        {
            get
            {
                return this._waveStream.WaveFormat;
            }
        }

        /// <summary>Playback State</summary>
        public PlaybackState PlaybackState
        {
            get
            {
                return this._playbackState;
            }
        }

        /// <summary>Volume for this device 1.0 is full scale</summary>
        public float Volume
        {
            get
            {
                return AudioUtils.GetWaveOutVolume(this._hWaveOut, this._waveOutLock);
            }
            set
            {
                AudioUtils.SetWaveOutVolume(value, this._hWaveOut, this._waveOutLock);
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
            if (this._callbackEvent != null)
            {
                this._callbackEvent.Close();
                this._callbackEvent = (AutoResetEvent)null;
            }
            lock (this._waveOutLock)
            {
                if (!(this._hWaveOut != IntPtr.Zero))
                    return;
                int num = (int)AudioInterop.waveOutClose(this._hWaveOut);
                this._hWaveOut = IntPtr.Zero;
            }
        }

        private void DisposeBuffers()
        {
            if (this._buffers == null)
                return;
            foreach (AudioBuffer buffer in this._buffers)
                buffer.Dispose();
            this._buffers = (AudioBuffer[])null;
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
            if (this._syncContext == null)
                handler((object)this, new StoppedEventArgs(e));
            else
                this._syncContext.Post((SendOrPostCallback)(state => handler((object)this, new StoppedEventArgs(e))), (object)null);
        }
    }
}