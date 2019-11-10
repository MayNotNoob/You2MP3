using System;
using System.Runtime.InteropServices;
using NAudio;

namespace U2MP3.AudioEngine
{
    public static class AudioUtils
    {
        public static float GetWaveOutVolume(IntPtr hWaveOut, object lockObject)
        {
            int dwVolume;
            MmResult volume;
            lock (lockObject)
                volume = AudioInterop.waveOutGetVolume(hWaveOut, out dwVolume);
            MmException.Try(volume, "waveOutGetVolume");
            return (dwVolume & ushort.MaxValue) / (float)ushort.MaxValue;
        }

        public static void SetWaveOutVolume(float value, IntPtr hWaveOut, object lockObject)
        {
            if (value < 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
            if (value > 1.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
            int dwVolume = (int)(value * (double)ushort.MaxValue) + ((int)(value * (double)ushort.MaxValue) << 16);
            MmResult result;
            lock (lockObject)
                result = AudioInterop.waveOutSetVolume(hWaveOut, dwVolume);
            MmException.Try(result, "waveOutSetVolume");
        }

        public static long GetPositionBytes(IntPtr hWaveOut, object lockObject)
        {
            lock (lockObject)
            {
                MmTime mmTime = new MmTime();
                mmTime.wType = 4U;
                MmException.Try(AudioInterop.waveOutGetPosition(hWaveOut, ref mmTime, Marshal.SizeOf((object)mmTime)), "waveOutGetPosition");
                if (mmTime.wType != 4U)
                    throw new Exception($"waveOutGetPosition: wType -> Expected {4}, Received {mmTime.wType}");
                return mmTime.cb;
            }
        }
    }
}