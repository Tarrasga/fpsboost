using System;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace FPSBoostPro.Core.Monitoring
{
    public record SystemSpecs(
        string CpuName,
        string GpuName,
        string RamInfo,
        string WindowsVersion,
        string ActivePowerPlan,
        bool IsAdmin
    );

    public class SystemInfoProvider
    {
        public static bool CheckIsAdministrator()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static SystemSpecs GetSystemSpecifications()
        {
            string cpuName = GetCpuModel();
            string gpuName = GetGpuModel();
            var mem = Tweaks.MemoryOptimizer.GetCurrentMemoryStats();
            string ramInfo = $"{mem.TotalGb:F1} GB Total ({mem.UsedGb:F1} GB em uso)";
            string winVersion = GetWindowsVersion();
            string powerPlan = GetActivePowerPlan();
            bool isAdmin = CheckIsAdministrator();

            return new SystemSpecs(cpuName, gpuName, ramInfo, winVersion, powerPlan, isAdmin);
        }

        private static string GetCpuModel()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var name = key?.GetValue("ProcessorNameString") as string;
                if (!string.IsNullOrEmpty(name))
                {
                    return name.Trim();
                }
            }
            catch { }
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Processador Desconhecido";
        }

        private static string GetGpuModel()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000");
                var driverDesc = key?.GetValue("DriverDesc") as string;
                if (!string.IsNullOrEmpty(driverDesc))
                {
                    return driverDesc.Trim();
                }
            }
            catch { }
            return "Placa Gráfica Detetada";
        }

        private static string GetWindowsVersion()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var productName = key?.GetValue("ProductName") as string;
                var displayVersion = key?.GetValue("DisplayVersion") as string;
                if (!string.IsNullOrEmpty(productName))
                {
                    return $"{productName} {displayVersion}".Trim();
                }
            }
            catch { }
            return $"Windows {Environment.OSVersion.Version.Major}";
        }

        private static string GetActivePowerPlan()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "-getactivescheme",
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                string outStr = p?.StandardOutput.ReadToEnd() ?? "";
                p?.WaitForExit(3000);

                int openParen = outStr.IndexOf('(');
                int closeParen = outStr.IndexOf(')');
                if (openParen != -1 && closeParen > openParen)
                {
                    return outStr.Substring(openParen + 1, closeParen - openParen - 1);
                }
            }
            catch { }
            return "Plano Padrão";
        }

        public static void RestartAsAdministrator()
        {
            try
            {
                string? exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return;

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                Environment.Exit(0);
            }
            catch { }
        }
    }
}
