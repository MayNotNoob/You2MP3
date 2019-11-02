using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace U2MP3.AudioEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public class WaveHeader
    {
        /// <summary>pointer to locked data buffer (lpData)</summary>
        public IntPtr dataBuffer;
        /// <summary>length of data buffer (dwBufferLength)</summary>
        public int bufferLength;
        /// <summary>used for input only (dwBytesRecorded)</summary>
        public int bytesRecorded;
        /// <summary>for client's use (dwUser)</summary>
        public IntPtr userData;
        /// <summary>assorted flags (dwFlags)</summary>
        public WaveHeaderFlags flags;
        /// <summary>loop control counter (dwLoops)</summary>
        public int loops;
        /// <summary>PWaveHdr, reserved for driver (lpNext)</summary>
        public IntPtr next;
        /// <summary>reserved for driver</summary>
        public IntPtr reserved;
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct MmTime
    {
        public const int TIME_MS = 1;
        public const int TIME_SAMPLES = 2;
        public const int TIME_BYTES = 4;
        [FieldOffset(0)]
        public uint wType;
        [FieldOffset(4)]
        public uint ms;
        [FieldOffset(4)]
        public uint sample;
        [FieldOffset(4)]
        public uint cb;
        [FieldOffset(4)]
        public uint ticks;
        [FieldOffset(4)]
        public byte smpteHour;
        [FieldOffset(5)]
        public byte smpteMin;
        [FieldOffset(6)]
        public byte smpteSec;
        [FieldOffset(7)]
        public byte smpteFrame;
        [FieldOffset(8)]
        public byte smpteFps;
        [FieldOffset(9)]
        public byte smpteDummy;
        [FieldOffset(10)]
        public byte smptePad0;
        [FieldOffset(11)]
        public byte smptePad1;
        [FieldOffset(4)]
        public uint midiSongPtrPos;
    }
}