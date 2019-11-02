using System;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace U2MP3.AudioEngine
{
    public class AudioBuffer
    {
        private readonly WaveHeader _header;
        private readonly byte[] _buffer;
        private readonly IWaveProvider _waveStream;
        private readonly object _waveOutLock;
        private GCHandle _hBuffer;
        private IntPtr _hWaveOut;
        private GCHandle _hHeader;
        private GCHandle _hThis;

        /// <summary>creates a new wavebuffer</summary>
        /// <param name="hWaveOut">WaveOut device to write to</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="bufferFillStream">Stream to provide more data</param>
        /// <param name="waveOutLock">Lock to protect WaveOut API's from being called on &gt;1 thread</param>
        public AudioBuffer(
          IntPtr hWaveOut,
          int bufferSize,
          IWaveProvider bufferFillStream,
          object waveOutLock)
        {
            BufferSize = bufferSize;
            _buffer = new byte[bufferSize];
            _hBuffer = GCHandle.Alloc((object)_buffer, GCHandleType.Pinned);
            _hWaveOut = hWaveOut;
            _waveStream = bufferFillStream;
            _waveOutLock = waveOutLock;
            _header = new WaveHeader();
            _hHeader = GCHandle.Alloc((object)_header, GCHandleType.Pinned);
            _header.dataBuffer = _hBuffer.AddrOfPinnedObject();
            _header.bufferLength = bufferSize;
            _header.loops = 1;
            _hThis = GCHandle.Alloc((object)this);
            _header.userData = (IntPtr)_hThis;
            lock (waveOutLock)
                MmException.Try(AudioInterop.waveOutPrepareHeader(hWaveOut, _header, Marshal.SizeOf((object)_header)), "waveOutPrepareHeader");
        }

        /// <summary>Finalizer for this wave buffer</summary>
        ~AudioBuffer()
        {
            Dispose(false);
        }

        /// <summary>Releases resources held by this WaveBuffer</summary>
        public void Dispose()
        {
            GC.SuppressFinalize((object)this);
            Dispose(true);
        }

        /// <summary>Releases resources held by this WaveBuffer</summary>
        protected void Dispose(bool disposing)
        {
            int num1 = disposing ? 1 : 0;
            if (_hHeader.IsAllocated)
                _hHeader.Free();
            if (_hBuffer.IsAllocated)
                _hBuffer.Free();
            if (_hThis.IsAllocated)
                _hThis.Free();
            if (!(_hWaveOut != IntPtr.Zero))
                return;
            lock (_waveOutLock)
            {
                int num2 = (int)AudioInterop.waveOutUnprepareHeader(_hWaveOut, _header, Marshal.SizeOf((object)_header));
            }
            _hWaveOut = IntPtr.Zero;
        }

        /// this is called by the WAVE callback and should be used to refill the buffer
        internal bool OnDone()
        {
            int num;
            lock (_waveStream)
                num = _waveStream.Read(_buffer, 0, _buffer.Length);
            if (num == 0)
                return false;
            for (int index = num; index < _buffer.Length; ++index)
                _buffer[index] = (byte)0;
            WriteToWaveOut();
            return true;
        }

        /// <summary>Whether the header's in queue flag is set</summary>
        public bool InQueue => (_header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;

        /// <summary>The buffer size in bytes</summary>
        public int BufferSize { get; }

        private void WriteToWaveOut()
        {
            MmResult result;
            lock (_waveOutLock)
                result = AudioInterop.waveOutWrite(_hWaveOut, _header, Marshal.SizeOf((object)_header));
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutWrite");
            GC.KeepAlive((object)this);
        }
    }
}