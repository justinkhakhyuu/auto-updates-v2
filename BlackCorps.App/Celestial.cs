using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celestial
{
    // --- ENUMS ---
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        AllAccess = 0x1F0FFF,
        Terminate = 0x1,
        CreateThread = 0x2,
        VmOperation = 0x8,
        VmRead = 0x10,
        VmWrite = 0x20,
        DupHandle = 0x40,
        SetInformation = 0x200,
        QueryInformation = 0x400,
        SuspendResume = 0x800,
        Synchronize = 0x100000
    }

    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = 0x0001,
        SUSPEND_RESUME = 0x0002,
        GET_CONTEXT = 0x0008,
        SET_CONTEXT = 0x0010,
        SET_INFORMATION = 0x0020,
        QUERY_INFORMATION = 0x0040,
        SET_THREAD_TOKEN = 0x0080,
        IMPERSONATE = 0x0100,
        DIRECT_IMPERSONATION = 0x0200
    }

    public class Cosmic
    {
        // --- STRUCTS ---
        public struct PatternData
        {
            public byte[] pattern { get; set; }
            public byte[] mask { get; set; }
        }

        public struct MemoryPage
        {
            public IntPtr Start;
            public int Size;
            public MemoryPage(IntPtr start, int size)
            {
                Start = start;
                Size = size;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        // --- VARIABLES ---
        public int processId;
        public IntPtr _processHandle;
        private bool _enableCheck = true;

        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_PRIVATE = 0x20000;
        public const uint PAGE_READWRITE = 0x04;

        // --- DLL IMPORTS ---
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        // --- METHODS ---

        // Menghubungkan ke Process (cth: HD-Player)
        public bool SetProcess(string[] processNames)
        {
            processId = 0;
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (Array.Exists(processNames, name => name.Equals(process.ProcessName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    processId = process.Id;
                    break;
                }
            }

            if (processId <= 0) return false;

            _processHandle = OpenProcess(ProcessAccessFlags.AllAccess, false, processId);
            return _processHandle != IntPtr.Zero;
        }

        // Fungsi Utama AoB Scan (Array of Bytes)
        public async Task<IEnumerable<long>> AoBScan(string bytePattern)
        {
            PatternData patternData = GetPatternDataFromPattern(bytePattern);
            List<long> addressRet = new List<long>();

            await Task.Run(() =>
            {
                List<MemoryPage> memoryPages = new List<MemoryPage>();
                IntPtr currentAddr = IntPtr.Zero;
                MEMORY_BASIC_INFORMATION memInfo;

                // 1. Petakan seluruh region memory process
                while (VirtualQueryEx(_processHandle, currentAddr, out memInfo, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
                {
                    // Hanya scan memory yang COMMITTED (Aktif) dan bisa dibaca/tulis
                    if (memInfo.State == MEM_COMMIT && (memInfo.Protect == PAGE_READWRITE || memInfo.Protect == 0x20 || memInfo.Protect == 0x40)) 
                    {
                        memoryPages.Add(new MemoryPage(memInfo.BaseAddress, (int)memInfo.RegionSize));
                    }
                    
                    // Pindah ke region berikutnya (Hindari overflow di 64bit)
                    long nextVal = (long)memInfo.BaseAddress + (long)memInfo.RegionSize;
                    if (nextVal <= (long)currentAddr) break; // Break jika overflow
                    currentAddr = (IntPtr)nextVal;
                }

                // 2. Scan setiap page secara Parallel (Multi-threading)
                Parallel.ForEach(memoryPages, page =>
                {
                    byte[] buffer = new byte[page.Size];
                    IntPtr bytesRead;

                    if (ReadProcessMemory(_processHandle, page.Start, buffer, (IntPtr)page.Size, out bytesRead))
                    {
                        int offset = -patternData.pattern.Length;
                        do
                        {
                            offset = FindPattern(buffer, patternData.pattern, patternData.mask, offset + patternData.pattern.Length);
                            if (offset >= 0)
                            {
                                lock (addressRet)
                                {
                                    addressRet.Add((long)page.Start + offset);
                                }
                            }
                        } while (offset != -1);
                    }
                });
            });

            return addressRet.OrderBy(c => c).AsEnumerable();
        }

        // Helper untuk memparsing pattern (contoh: "A0 ?? ?? FF")
        private PatternData GetPatternDataFromPattern(string pattern)
        {
            string[] source = pattern.Split(' ');
            return new PatternData
            {
                pattern = source.Select(s => s.Contains("??") ? (byte)0 : byte.Parse(s, NumberStyles.HexNumber)).ToArray(),
                mask = source.Select(s => s.Contains("??") ? (byte)0 : (byte)0xFF).ToArray()
            };
        }

        // Algoritma pencarian pattern byte
        private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
        {
            if (body.Length == 0 || pattern.Length == 0 || start > body.Length - pattern.Length) return -1;

            for (int i = start; i <= body.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    // Jika mask 0 (??), skip pengecekan. Jika mask FF, harus sama persis.
                    if (masks[j] != 0 && body[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return i;
            }
            return -1;
        }

        // --- Helper Write/Read ---
        public bool WriteMemory(long address, byte[] bytes)
        {
            return WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, IntPtr.Zero);
        }

        public bool ReadMemory(long address, byte[] buffer)
        {
            return ReadProcessMemory(_processHandle, (IntPtr)address, buffer, (IntPtr)buffer.Length, out var _);
        }
    }
}
