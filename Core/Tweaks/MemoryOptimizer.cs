using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FPSBoostPro.Core.Tweaks
{
    public class MemoryOptimizer
    {
        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public record MemoryStats(double TotalGb, double FreeGb, double UsedGb, int UsedPercentage);

        public static MemoryStats GetCurrentMemoryStats()
        {
            var stat = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(stat))
            {
                double totalGb = stat.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                double freeGb = stat.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                double usedGb = totalGb - freeGb;
                return new MemoryStats(totalGb, freeGb, usedGb, (int)stat.dwMemoryLoad);
            }
            return new MemoryStats(16, 8, 8, 50);
        }

        public static (bool Success, string Message, double MbFreed) OptimizeMemory()
        {
            var before = GetCurrentMemoryStats();
            int trimmedProcesses = 0;

            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    // Não limitar processos do sistema crítico
                    string name = proc.ProcessName.ToLowerInvariant();
                    if (name is "system" or "idle" or "csrss" or "smss" or "lsass" or "services" or "wininit" or "svchost")
                    {
                        continue;
                    }

                    // Focar principalmente em aplicações em segundo plano que devoram RAM
                    // (Chrome, Edge, Discord, Spotify, etc.)
                    EmptyWorkingSet(proc.Handle);
                    trimmedProcesses++;
                }
                catch
                {
                    // Processos protegidos ignoram-se normalmente
                }
                finally
                {
                    proc.Dispose();
                }
            }

            // Forçar Garbage Collector da nossa própria app também
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var after = GetCurrentMemoryStats();
            double mbFreed = Math.Max(0, (before.UsedGb - after.UsedGb) * 1024.0);

            return (true, $"Memória otimizada! {trimmedProcesses} processos limpos. ({mbFreed:F0} MB libertados)", mbFreed);
        }
    }
}
