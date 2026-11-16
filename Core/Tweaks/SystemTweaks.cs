using System;
using System.Diagnostics;
using System.IO;
using FPSBoostPro.Core.Safety;
using Microsoft.Win32;

namespace FPSBoostPro.Core.Tweaks
{
    public class SystemTweaks
    {
        public static (bool Success, string Message) EnableUltimatePerformance()
        {
            try
            {
                // Duplica o plano de Desempenho Máximo (Ultimate Performance)
                var p1 = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61",
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                string output = p1?.StandardOutput.ReadToEnd() ?? "";
                p1?.WaitForExit(5000);

                // Ativa o plano de desempenho máximo
                var p2 = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "-setactive e9a42b02-d5df-448d-aa00-03f14749eb61",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                p2?.WaitForExit(5000);

                if (p2?.ExitCode != 0)
                {
                    // Fallback para Alto Desempenho (High Performance) padrão
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = "-setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit(5000);
                }

                return (true, "Plano de Desempenho Máximo / Alto Desempenho ativado com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao configurar energia: {ex.Message}");
            }
        }

        public static (bool Success, string Message) DisableGameDVR()
        {
            try
            {
                // 1. HKCU GameConfigStore
                const string configStoreKey = @"System\GameConfigStore";
                BackupManager.SaveOriginalValue("HKCU", configStoreKey, "GameDVR_Enabled");
                BackupManager.SaveOriginalValue("HKCU", configStoreKey, "GameDVR_FSEBehavior");
                BackupManager.SaveOriginalValue("HKCU", configStoreKey, "GameDVR_HonorUserFSEBehaviorMode");
                BackupManager.SaveOriginalValue("HKCU", configStoreKey, "GameDVR_DXGIHonorFSEWindowsCompatible");

                using (var key = Registry.CurrentUser.CreateSubKey(configStoreKey))
                {
                    key?.SetValue("GameDVR_Enabled", 0, RegistryValueKind.DWord);
                    key?.SetValue("GameDVR_FSEBehavior", 2, RegistryValueKind.DWord);
                    key?.SetValue("GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord);
                    key?.SetValue("GameDVR_DXGIHonorFSEWindowsCompatible", 1, RegistryValueKind.DWord);
                }

                // 2. HKLM Policies GameDVR
                const string policiesKey = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";
                try
                {
                    BackupManager.SaveOriginalValue("HKLM", policiesKey, "AllowGameDVR");
                    using var polKey = Registry.LocalMachine.CreateSubKey(policiesKey);
                    polKey?.SetValue("AllowGameDVR", 0, RegistryValueKind.DWord);
                }
                catch { /* Requer permissões de administrador */ }

                // 3. HKCU GameDVR AppCapture
                const string appCaptureKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR";
                BackupManager.SaveOriginalValue("HKCU", appCaptureKey, "AppCaptureEnabled");
                BackupManager.SaveOriginalValue("HKCU", appCaptureKey, "HistoricalCaptureEnabled");

                using (var appKey = Registry.CurrentUser.CreateSubKey(appCaptureKey))
                {
                    appKey?.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
                    appKey?.SetValue("HistoricalCaptureEnabled", 0, RegistryValueKind.DWord);
                }

                return (true, "Xbox Game DVR & Gravação em Segundo Plano desativados! (Fim do stuttering)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao desativar Game DVR: {ex.Message}");
            }
        }

        public static (bool Success, string Message) OptimizeSystemResponsiveness()
        {
            try
            {
                const string multimediaKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
                BackupManager.SaveOriginalValue("HKLM", multimediaKey, "SystemResponsiveness");
                BackupManager.SaveOriginalValue("HKLM", multimediaKey, "NetworkThrottlingIndex");

                using (var key = Registry.LocalMachine.CreateSubKey(multimediaKey))
                {
                    // 0 = 100% de prioridade do processador para jogos em primeiro plano
                    key?.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
                    // 0xffffffff = desativa limitação de rede multimídia do Windows
                    key?.SetValue("NetworkThrottlingIndex", unchecked((int)0xffffffff), RegistryValueKind.DWord);
                }

                const string gamesKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";
                BackupManager.SaveOriginalValue("HKLM", gamesKey, "GPU Priority");
                BackupManager.SaveOriginalValue("HKLM", gamesKey, "Priority");
                BackupManager.SaveOriginalValue("HKLM", gamesKey, "Scheduling Category");
                BackupManager.SaveOriginalValue("HKLM", gamesKey, "SFIO Priority");

                using (var gKey = Registry.LocalMachine.CreateSubKey(gamesKey))
                {
                    gKey?.SetValue("GPU Priority", 8, RegistryValueKind.DWord);
                    gKey?.SetValue("Priority", 6, RegistryValueKind.DWord);
                    gKey?.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                    gKey?.SetValue("SFIO Priority", "High", RegistryValueKind.String);
                }

                return (true, "Prioridade Multimídia configurada no Máximo para Jogos (SystemResponsiveness = 0)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro na prioridade de sistema: {ex.Message} (Execute como Administrador)");
            }
        }

        public static (bool Success, string Message) DisableMouseAcceleration()
        {
            try
            {
                const string mouseKey = @"Control Panel\Mouse";
                BackupManager.SaveOriginalValue("HKCU", mouseKey, "MouseSpeed");
                BackupManager.SaveOriginalValue("HKCU", mouseKey, "MouseThreshold1");
                BackupManager.SaveOriginalValue("HKCU", mouseKey, "MouseThreshold2");

                using var key = Registry.CurrentUser.CreateSubKey(mouseKey);
                key?.SetValue("MouseSpeed", "0", RegistryValueKind.String);
                key?.SetValue("MouseThreshold1", "0", RegistryValueKind.String);
                key?.SetValue("MouseThreshold2", "0", RegistryValueKind.String);

                return (true, "Aceleração do Rato desativada (Sensibilidade 1:1 perfeita para CS2, Rust e PUBG)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao configurar rato: {ex.Message}");
            }
        }

        public static (bool Success, string Message) EnableHagsAndGpuOptimization()
        {
            try
            {
                const string graphicsDriversKey = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
                BackupManager.SaveOriginalValue("HKLM", graphicsDriversKey, "HwSchMode");

                using var key = Registry.LocalMachine.CreateSubKey(graphicsDriversKey);
                key?.SetValue("HwSchMode", 2, RegistryValueKind.DWord);

                return (true, "Agendamento de GPU por Hardware (HAGS) ativado com sucesso! (Menor latência de renderização da GPU - requer reiniciar o PC)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao configurar GPU: {ex.Message} (Execute como Administrador)");
            }
        }

        public static (bool Success, string Message, long BytesCleaned) CleanJunkAndShaderCache()
        {
            long bytesFreed = 0;
            int filesDeleted = 0;

            string[] pathsToClean = new[]
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "DXCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "DxCache")
            };

            foreach (var dir in pathsToClean)
            {
                if (!Directory.Exists(dir)) continue;

                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            long len = file.Length;
                            file.Delete();
                            bytesFreed += len;
                            filesDeleted++;
                        }
                        catch { /* Ficheiros em uso pelo Windows ignoram-se com segurança */ }
                    }
                }
                catch { }
            }

            double mbFreed = bytesFreed / (1024.0 * 1024.0);
            return (true, $"Limpeza concluída: {filesDeleted} ficheiros apagados ({mbFreed:F1} MB libertados)!", bytesFreed);
        }
    }
}
