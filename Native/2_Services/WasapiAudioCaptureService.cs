// ==============================================================================
// jDroid-X-Desk Native WASAPI Audio Loopback Capture Service
// Captures system speaker audio output using Windows Core Audio WASAPI Loopback
// Pure C# .NET 4.0/4.8 Interop (Zero External Dependencies)
// ==============================================================================

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace JDroidXDesk.Native
{
    public class WasapiAudioCaptureService
    {
        private static WasapiAudioCaptureService _instance;
        private static readonly object _lock = new object();

        public static WasapiAudioCaptureService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) _instance = new WasapiAudioCaptureService();
                    return _instance;
                }
            }
        }

        private Thread _captureThread;
        private volatile bool _isRunning = false;
        private byte[] _latestWavChunk = null;
        private readonly object _chunkLock = new object();
        private long _chunksCaptured = 0;

        public bool IsRunning { get { return _isRunning; } }
        public long ChunksCaptured { get { return _chunksCaptured; } }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _captureThread = new Thread(new ThreadStart(CaptureWorker))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _captureThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            if (_captureThread != null && _captureThread.IsAlive)
            {
                try { _captureThread.Join(500); } catch { }
            }
            _captureThread = null;
        }

        public byte[] GetLatestWavChunk()
        {
            lock (_chunkLock)
            {
                return _latestWavChunk;
            }
        }

        private void SetLatestWavChunk(byte[] chunk)
        {
            lock (_chunkLock)
            {
                _latestWavChunk = chunk;
                _chunksCaptured++;
            }
        }

        // ==========================================
        // WASAPI COM INTERFACES (Core Audio Windows Vista/7/8/10/11)
        // ==========================================
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        [ComImport]
        private class MMDeviceEnumeratorComObject { }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IMMDeviceEnumerator
        {
            int NotUsed1();
            [PreserveSig]
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppDevice);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [Guid("1CB9AD4C-DBFA-4c32-B12D-9880A77B16AD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IAudioClient
        {
            [PreserveSig]
            int Initialize(int shareMode, int streamFlags, long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat, ref Guid audioSessionGuid);
            [PreserveSig]
            int GetBufferSize(out uint pNumBufferFrames);
            [PreserveSig]
            int GetStreamLatency(out long phnsLatency);
            [PreserveSig]
            int GetCurrentPadding(out uint pNumPaddingFrames);
            [PreserveSig]
            int IsFormatSupported(int shareMode, IntPtr pFormat, out IntPtr ppClosestMatch);
            [PreserveSig]
            int GetMixFormat(out IntPtr ppDeviceFormat);
            [PreserveSig]
            int GetDevicePeriod(out long phnsDefaultDevicePeriod, out long phnsMinimumDevicePeriod);
            [PreserveSig]
            int Start();
            [PreserveSig]
            int Stop();
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int SetEventHandle(IntPtr eventHandle);
            [PreserveSig]
            int GetService(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        }

        [Guid("C8ADBD64-E71E-48a0-A4DE-185C9F723831")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface IAudioCaptureClient
        {
            [PreserveSig]
            int GetBuffer(out IntPtr ppData, out uint pNumFramesToRead, out uint pdwFlags, out ulong pu64DevicePosition, out ulong pu64QPCPosition);
            [PreserveSig]
            int ReleaseBuffer(uint numFramesRead);
            [PreserveSig]
            int GetNextPacketSize(out uint pNumFramesInNextPacket);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        private const int eRender = 0;
        private const int eMultimedia = 1;
        private const int CLSCTX_ALL = 23;
        private const int AUDCLNT_SHAREMODE_SHARED = 0;
        private const int AUDCLNT_STREAMFLAGS_LOOPBACK = 0x00020000;

        private void CaptureWorker()
        {
            IMMDevice device = null;
            IAudioClient audioClient = null;
            IAudioCaptureClient captureClient = null;
            IntPtr pMixFormat = IntPtr.Zero;

            try
            {
                IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int hr = enumerator.GetDefaultAudioEndpoint(eRender, eMultimedia, out device);
                if (hr != 0 || device == null) return;

                Guid audioClientGuid = new Guid("1CB9AD4C-DBFA-4c32-B12D-9880A77B16AD");
                object clientObj;
                hr = device.Activate(ref audioClientGuid, CLSCTX_ALL, IntPtr.Zero, out clientObj);
                if (hr != 0 || clientObj == null) return;

                audioClient = (IAudioClient)clientObj;
                hr = audioClient.GetMixFormat(out pMixFormat);
                if (hr != 0 || pMixFormat == IntPtr.Zero) return;

                WAVEFORMATEX format = (WAVEFORMATEX)Marshal.PtrToStructure(pMixFormat, typeof(WAVEFORMATEX));
                Guid emptyGuid = Guid.Empty;

                // 200ms buffer
                long hnsRequestedDuration = 2000000;
                hr = audioClient.Initialize(AUDCLNT_SHAREMODE_SHARED, AUDCLNT_STREAMFLAGS_LOOPBACK, hnsRequestedDuration, 0, pMixFormat, ref emptyGuid);
                if (hr != 0) return;

                Guid captureClientGuid = new Guid("C8ADBD64-E71E-48a0-A4DE-185C9F723831");
                object captureObj;
                hr = audioClient.GetService(ref captureClientGuid, out captureObj);
                if (hr != 0 || captureObj == null) return;

                captureClient = (IAudioCaptureClient)captureObj;
                hr = audioClient.Start();
                if (hr != 0) return;

                int sampleRate = (int)format.nSamplesPerSec;
                int channels = (int)format.nChannels;
                int bitsPerSample = (int)format.wBitsPerSample;
                int blockAlign = (int)format.nBlockAlign;

                using (MemoryStream pcmAccumulator = new MemoryStream())
                {
                    DateTime lastFlushTime = DateTime.UtcNow;

                    while (_isRunning)
                    {
                        uint packetLength = 0;
                        hr = captureClient.GetNextPacketSize(out packetLength);
                        if (hr == 0 && packetLength > 0)
                        {
                            IntPtr pData;
                            uint numFramesRead;
                            uint flags;
                            ulong devPos, qpcPos;

                            hr = captureClient.GetBuffer(out pData, out numFramesRead, out flags, out devPos, out qpcPos);
                            if (hr == 0 && numFramesRead > 0)
                            {
                                int bytesToRead = (int)(numFramesRead * blockAlign);
                                byte[] raw = new byte[bytesToRead];
                                if ((flags & 0x00000002) != 0) // AUDCLNT_BUFFERFLAGS_SILENT
                                {
                                    Array.Clear(raw, 0, raw.Length);
                                }
                                else
                                {
                                    Marshal.Copy(pData, raw, 0, bytesToRead);
                                }
                                captureClient.ReleaseBuffer(numFramesRead);
                                pcmAccumulator.Write(raw, 0, raw.Length);
                            }
                        }

                        // Flush chunks every 120ms
                        if ((DateTime.UtcNow - lastFlushTime).TotalMilliseconds >= 120)
                        {
                            if (pcmAccumulator.Length > 0)
                            {
                                byte[] pcmData = pcmAccumulator.ToArray();
                                pcmAccumulator.SetLength(0);
                                byte[] wavData = BuildWavFile(pcmData, sampleRate, (short)channels, (short)bitsPerSample);
                                SetLatestWavChunk(wavData);
                            }
                            lastFlushTime = DateTime.UtcNow;
                        }

                        Thread.Sleep(15);
                    }
                }
            }
            catch { }
            finally
            {
                try { if (audioClient != null) audioClient.Stop(); } catch { }
                if (pMixFormat != IntPtr.Zero) Marshal.FreeCoTaskMem(pMixFormat);
            }
        }

        private static byte[] BuildWavFile(byte[] pcmData, int sampleRate, short channels, short bitsPerSample)
        {
            using (MemoryStream ms = new MemoryStream(pcmData.Length + 44))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                int byteRate = sampleRate * channels * (bitsPerSample / 8);
                short blockAlign = (short)(channels * (bitsPerSample / 8));

                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write((int)(36 + pcmData.Length));
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write((int)16); // SubChunk1Size (16 for PCM)
                bw.Write((short)(bitsPerSample == 32 ? 3 : 1)); // 1 = PCM, 3 = IEEE Float
                bw.Write((short)channels);
                bw.Write((int)sampleRate);
                bw.Write((int)byteRate);
                bw.Write((short)blockAlign);
                bw.Write((short)bitsPerSample);

                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write((int)pcmData.Length);
                bw.Write(pcmData);

                return ms.ToArray();
            }
        }
    }
}
