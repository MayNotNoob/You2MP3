using System;
using NAudio.Wave;

namespace LibTest
{
    public class SavingWaveProvider : IWaveProvider, IDisposable
    {
        private readonly IWaveProvider _waveProvider;
        private readonly WaveFileWriter _waveFileWriter;
        private bool isWriterDisposed;
        public SavingWaveProvider(IWaveProvider waveProvider, string filePath)
        {
            _waveProvider = waveProvider;
            _waveFileWriter = new WaveFileWriter(filePath, _waveProvider.WaveFormat);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var read = _waveProvider.Read(buffer, offset, count);

            if (count > 0 && !isWriterDisposed)
            {
                _waveFileWriter.Write(buffer, offset, read);
            }

            if (count == 0)
            {
                Dispose(); // auto-dispose in case users forget
            }

            return read;
        }

        public WaveFormat WaveFormat => _waveProvider.WaveFormat;

        public void Dispose()
        {
            if (!isWriterDisposed)
            {
                isWriterDisposed = true;
                _waveFileWriter.Dispose();
            }
        }
    }
}