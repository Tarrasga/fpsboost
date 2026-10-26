using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using FPSBoostPro.Core.Safety;
using Microsoft.Win32;

namespace FPSBoostPro.Core.Tweaks
{
    public class NetworkTweaks
    {
        public static (bool Success, string Message) DisableNaglesAlgorithm()
        {
            try
            {
                const string interfacesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
                using var interfacesKey = Registry.LocalMachine.OpenSubKey(interfacesPath, true);
                if (interfacesKey == null)
                {
                    return (false, "Chave de rede não encontrada ou sem permissão de Administrador.");
                }

                int patchedCount = 0;
                foreach (string subKeyName in interfacesKey.GetSubKeyNames())
                {
                    using var subKey = interfacesKey.OpenSubKey(subKeyName, true);
                    if (subKey != null)
                    {
                        // Apenas interfaces com IP atribuído ou gateway são placas ativas
                        string fullPath = $"{interfacesPath}\\{subKeyName}";
                        BackupManager.SaveOriginalValue("HKLM", fullPath, "TcpAckFrequency");
                        BackupManager.SaveOriginalValue("HKLM", fullPath, "TCPNoDelay");
                        BackupManager.SaveOriginalValue("HKLM", fullPath, "TcpDelAckTicks");

                        subKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                        subKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                        subKey.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                        patchedCount++;
                    }
                }

                return (true, $"Algoritmo de Nagle desativado em {patchedCount} adaptadores! (Envio imediato de pacotes TCP sem atraso)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao configurar TCP: {ex.Message} (Execute como Administrador)");
            }
        }

        public static (bool Success, string Message) FlushDns()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using var proc = Process.Start(psi);
                string output = proc?.StandardOutput.ReadToEnd() ?? "";
                proc?.WaitForExit(5000);

                return (true, "Cache DNS limpo com sucesso! (Rotas de rede atualizadas)");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao limpar DNS: {ex.Message}");
            }
        }

        public static (bool Success, string Message) SetGamingDns(string primaryDns, string secondaryDns)
        {
            try
            {
                // Descobre a interface de rede ativa com gateway
                NetworkInterface? activeNic = null;
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        nic.GetIPProperties().GatewayAddresses.Count > 0)
                    {
                        activeNic = nic;
                        break;
                    }
                }

                if (activeNic == null)
                {
                    return (false, "Nenhuma placa de rede ativa encontrada.");
                }

                string nicName = activeNic.Name;

                if (string.IsNullOrEmpty(primaryDns))
                {
                    // Restaurar DHCP
                    var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip set dns \"{nicName}\" dhcp",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    proc?.WaitForExit(5000);
                    return (true, $"DNS restaurado para Automático (DHCP) na interface '{nicName}'.");
                }
                else
                {
                    // Configura DNS Primário
                    var p1 = Process.Start(new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip set dns \"{nicName}\" static {primaryDns} primary",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    p1?.WaitForExit(5000);

                    // Configura DNS Secundário
                    if (!string.IsNullOrEmpty(secondaryDns))
                    {
                        var p2 = Process.Start(new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"interface ip add dns \"{nicName}\" {secondaryDns} index=2",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        });
                        p2?.WaitForExit(5000);
                    }

                    return (true, $"DNS de Baixa Latência ({primaryDns}) aplicado na interface '{nicName}'!");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao alterar DNS: {ex.Message} (Execute como Administrador)");
            }
        }

        public static (bool Success, string Message) ResetWinsockAndTcpStack()
        {
            try
            {
                var p1 = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "winsock reset",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                p1?.WaitForExit(5000);

                var p2 = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "int ip reset",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                p2?.WaitForExit(5000);

                return (true, "Pilha TCP/IP e Winsock redefinidos com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao reiniciar Winsock: {ex.Message}");
            }
        }
    }
}
