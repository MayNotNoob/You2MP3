using System;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace LibFramTest
{
    public class MyBuffer : IDisposable
    {
        private readonly WaveHeader header;
        private readonly int bufferSize;
        private readonly byte[] buffer;
        private readonly IWaveProvider waveStream;
        private readonly object waveOutLock;
        private GCHandle hBuffer;
        private IntPtr hWaveOut;
        private GCHandle hHeader;
        private GCHandle hThis;

        /// <summary>creates a new wavebuffer</summary>
        /// <param name="hWaveOut">WaveOut device to write to</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="bufferFillStream">Stream to provide more data</param>
        /// <param name="waveOutLock">Lock to protect WaveOut API's from being called on &gt;1 thread</param>
        public MyBuffer(
          IntPtr hWaveOut,
          int bufferSize,
          IWaveProvider bufferFillStream,
          object waveOutLock)
        {
            this.bufferSize = bufferSize;
            this.buffer = new byte[bufferSize];
            this.hBuffer = GCHandle.Alloc((object)this.buffer, GCHandleType.Pinned);
            this.hWaveOut = hWaveOut;
            this.waveStream = bufferFillStream;
            this.waveOutLock = waveOutLock;
            this.header = new WaveHeader();
            this.hHeader = GCHandle.Alloc((object)this.header, GCHandleType.Pinned);
            this.header.dataBuffer = this.hBuffer.AddrOfPinnedObject();
            this.header.bufferLength = bufferSize;
            this.header.loops = 1;
            this.hThis = GCHandle.Alloc((object)this);
            this.header.userData = (IntPtr)this.hThis;
            lock (waveOutLock)
                MmException.Try(MyInterop.waveOutPrepareHeader(hWaveOut, this.header, Marshal.SizeOf((object)this.header)), "waveOutPrepareHeader");
        }

        /// <summary>Finalizer for this wave buffer</summary>
        ~MyBuffer()
        {
            this.Dispose(false);
        }

        /// <summary>Releases resources held by this WaveBuffer</summary>
        public void Dispose()
        {
            GC.SuppressFinalize((object)this);
            this.Dispose(true);
        }

        /// <summary>Releases resources held by this WaveBuffer</summary>
        protected void Dispose(bool disposing)
        {
            int num1 = disposing ? 1 : 0;
            if (this.hHeader.IsAllocated)
                this.hHeader.Free();
            if (this.hBuffer.IsAllocated)
                this.hBuffer.Free();
            if (this.hThis.IsAllocated)
                this.hThis.Free();
            if (!(this.hWaveOut != IntPtr.Zero))
                return;
            lock (this.waveOutLock)
            {
                int num2 = (int)MyInterop.waveOutUnprepareHeader(this.hWaveOut, this.header, Marshal.SizeOf((object)this.header));
            }
            this.hWaveOut = IntPtr.Zero;
        }

        /// this is called by the WAVE callback and should be used to refill the buffer
        internal bool OnDone()
        {
            int num;
            lock (this.waveStream)
                num = this.waveStream.Read(this.buffer, 0, this.buffer.Length);
            if (num == 0)
                return false;
            for (int index = num; index < this.buffer.Length; ++index)
                this.buffer[index] = (byte)0;
            this.WriteToWaveOut();
            return true;
        }

        /// <summary>Whether the header's in queue flag is set</summary>
        public bool InQueue
        {
            get
            {
                return (this.header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;
            }
        }

        /// <summary>The buffer size in bytes</summary>
        public int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
        }

        private void WriteToWaveOut()
        {
            MmResult result;
            lock (this.waveOutLock)
                result = MyInterop.waveOutWrite(this.hWaveOut, this.header, Marshal.SizeOf((object)this.header));
            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutWrite");
            GC.KeepAlive((object)this);
        }
    }
}