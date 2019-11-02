using System;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;

namespace U2MP3.AudioEngine
{
    public class AudioInterop
    {
        [DllImport("winmm.dll")]
        public static extern int mmioStringToFOURCC([MarshalAs(UnmanagedType.LPStr)] string s, int flags);

        [DllImport("winmm.dll")]
        public static extern int waveOutGetNumDevs();

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutPrepareHeader(
          IntPtr hWaveOut,
          WaveHeader lpWaveOutHdr,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutUnprepareHeader(
          IntPtr hWaveOut,
          WaveHeader lpWaveOutHdr,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutWrite(
          IntPtr hWaveOut,
          WaveHeader lpWaveOutHdr,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutOpen(
          out IntPtr hWaveOut,
          IntPtr uDeviceID,
          WaveFormat lpFormat,
          AudioInterop.WaveCallback dwCallback,
          IntPtr dwInstance,
          AudioInterop.WaveInOutOpenFlags dwFlags);

        [DllImport("winmm.dll", EntryPoint = "waveOutOpen")]
        public static extern MmResult waveOutOpenWindow(
          out IntPtr hWaveOut,
          IntPtr uDeviceID,
          WaveFormat lpFormat,
          IntPtr callbackWindowHandle,
          IntPtr dwInstance,
          AudioInterop.WaveInOutOpenFlags dwFlags);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutPause(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutRestart(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutGetPosition(
          IntPtr hWaveOut,
          ref MmTime mmTime,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutSetVolume(IntPtr hWaveOut, int dwVolume);

        [DllImport("winmm.dll")]
        public static extern MmResult waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern MmResult waveOutGetDevCaps(
          IntPtr deviceID,
          out WaveOutCapabilities waveOutCaps,
          int waveOutCapsSize);

        [DllImport("winmm.dll")]
        public static extern int waveInGetNumDevs();

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern MmResult waveInGetDevCaps(
          IntPtr deviceID,
          out WaveInCapabilities waveInCaps,
          int waveInCapsSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInAddBuffer(IntPtr hWaveIn, WaveHeader pwh, int cbwh);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInClose(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInOpen(
          out IntPtr hWaveIn,
          IntPtr uDeviceID,
          WaveFormat lpFormat,
          AudioInterop.WaveCallback dwCallback,
          IntPtr dwInstance,
          AudioInterop.WaveInOutOpenFlags dwFlags);

        [DllImport("winmm.dll", EntryPoint = "waveInOpen")]
        public static extern MmResult waveInOpenWindow(
          out IntPtr hWaveIn,
          IntPtr uDeviceID,
          WaveFormat lpFormat,
          IntPtr callbackWindowHandle,
          IntPtr dwInstance,
          AudioInterop.WaveInOutOpenFlags dwFlags);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInPrepareHeader(
          IntPtr hWaveIn,
          WaveHeader lpWaveInHdr,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInUnprepareHeader(
          IntPtr hWaveIn,
          WaveHeader lpWaveInHdr,
          int uSize);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInReset(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInStop(IntPtr hWaveIn);

        [DllImport("winmm.dll")]
        public static extern MmResult waveInGetPosition(
          IntPtr hWaveIn,
          out MmTime mmTime,
          int uSize);

        [Flags]
        public enum WaveInOutOpenFlags
        {
            /// <summary>
            /// CALLBACK_NULL
            /// No callback
            /// </summary>
            CallbackNull = 0,
            /// <summary>
            /// CALLBACK_FUNCTION
            /// dwCallback is a FARPROC
            /// </summary>
            CallbackFunction = 196608, // 0x00030000
                                       /// <summary>
                                       /// CALLBACK_EVENT
                                       /// dwCallback is an EVENT handle
                                       /// </summary>
            CallbackEvent = 327680, // 0x00050000
                                    /// <summary>
                                    /// CALLBACK_WINDOW
                                    /// dwCallback is a HWND
                                    /// </summary>
            CallbackWindow = 65536, // 0x00010000
                                    /// <summary>
                                    /// CALLBACK_THREAD
                                    /// callback is a thread ID
                                    /// </summary>
            CallbackThread = 131072, // 0x00020000
        }

        public enum WaveMessage
        {
            /// <summary>WOM_OPEN</summary>
            WaveOutOpen = 955, // 0x000003BB
                               /// <summary>WOM_CLOSE</summary>
            WaveOutClose = 956, // 0x000003BC
                                /// <summary>WOM_DONE</summary>
            WaveOutDone = 957, // 0x000003BD
                               /// <summary>WIM_OPEN</summary>
            WaveInOpen = 958, // 0x000003BE
                              /// <summary>WIM_CLOSE</summary>
            WaveInClose = 959, // 0x000003BF
                               /// <summary>WIM_DATA</summary>
            WaveInData = 960, // 0x000003C0
        }

        public delegate void WaveCallback(
          IntPtr hWaveOut,
          WaveMessage message,
          IntPtr dwInstance,
          WaveHeader wavhdr,
          IntPtr dwReserved);
    }
}