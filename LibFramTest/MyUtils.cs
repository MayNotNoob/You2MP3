using System;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace LibFramTest
{
    public static class MyUtils
    {
        public static float GetWaveOutVolume(IntPtr hWaveOut, object lockObject)
        {
            int dwVolume;
            MmResult volume;
            lock (lockObject)
                volume = MyInterop.waveOutGetVolume(hWaveOut, out dwVolume);
            MmException.Try(volume, "waveOutGetVolume");
            return (float)(dwVolume & (int)ushort.MaxValue) / (float)ushort.MaxValue;
        }

        public static void SetWaveOutVolume(float value, IntPtr hWaveOut, object lockObject)
        {
            if ((double)value < 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
            if ((double)value > 1.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
            int dwVolume = (int)((double)value * (double)ushort.MaxValue) + ((int)((double)value * (double)ushort.MaxValue) << 16);
            MmResult result;
            lock (lockObject)
                result = MyInterop.waveOutSetVolume(hWaveOut, dwVolume);
            MmException.Try(result, "waveOutSetVolume");
        }

        public static long GetPositionBytes(IntPtr hWaveOut, object lockObject)
        {
            lock (lockObject)
            {
                MmTime mmTime = new MmTime();
                mmTime.wType = 4U;
                MmException.Try(MyInterop.waveOutGetPosition(hWaveOut, ref mmTime, Marshal.SizeOf((object)mmTime)), "waveOutGetPosition");
                if (mmTime.wType != 4U)
                    throw new Exception(string.Format("waveOutGetPosition: wType -> Expected {0}, Received {1}", (object)4, (object)mmTime.wType));
                return (long)mmTime.cb;
            }
        }
    }
}