using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace FPSBoostPro.Core.Safety
{
    public class RegistryBackupEntry
    {
        public string Hive { get; set; } = "";
        public string SubKey { get; set; } = "";
        public string ValueName { get; set; } = "";
        public object? Value { get; set; }
        public string ValueKind { get; set; } = "";
    }

    public class BackupManager
    {
        private static readonly string BackupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FPSBoostPro",
            "Backups"
        );
        private static readonly string BackupFile = Path.Combine(BackupFolder, "registry_backup.json");

        public static (bool Success, string Message) CreateSystemRestorePoint()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Enable-ComputerRestore -Drive 'C:'; Checkpoint-Computer -Description 'FPSBoostPro_RestorePoint' -RestorePointType 'MODIFY_SETTINGS'\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit(30000);

                if (proc != null && proc.ExitCode == 0)
                {
                    return (true, "Ponto de Restauração criado com sucesso no Windows!");
                }
                return (false, "Não foi possível criar o ponto de restauração (permissão de Administrador necessária).");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao criar ponto de restauração: {ex.Message}");
            }
        }

        public static void SaveOriginalValue(string hiveName, string subKeyPath, string valueName)
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                {
                    Directory.CreateDirectory(BackupFolder);
                }

                var backups = LoadBackups();
                string key = $"{hiveName}\\{subKeyPath}\\{valueName}";

                // Se já existir backup deste valor, preserva o primeiro registro (original do Windows)
                if (backups.ContainsKey(key)) return;

                RegistryKey? baseKey = hiveName switch
                {
                    "HKLM" => Registry.LocalMachine,
                    "HKCU" => Registry.CurrentUser,
                    _ => null
                };

                if (baseKey == null) return;

                using var subKey = baseKey.OpenSubKey(subKeyPath, false);
                if (subKey != null)
                {
                    object? val = subKey.GetValue(valueName);
                    if (val != null)
                    {
                        var kind = subKey.GetValueKind(valueName).ToString();
                        backups[key] = new RegistryBackupEntry
                        {
                            Hive = hiveName,
                            SubKey = subKeyPath,
                            ValueName = valueName,
                            Value = val,
                            ValueKind = kind
                        };
                    }
                }

                string json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(BackupFile, json);
            }
            catch
            {
                // Backup silencioso caso ocorra restrição
            }
        }

        public static Dictionary<string, RegistryBackupEntry> LoadBackups()
        {
            try
            {
                if (File.Exists(BackupFile))
                {
                    string json = File.ReadAllText(BackupFile);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, RegistryBackupEntry>>(json);
                    if (dict != null) return dict;
                }
            }
            catch { }
            return new Dictionary<string, RegistryBackupEntry>();
        }

        public static (bool Success, string Message) RestoreOriginalDefaults()
        {
            try
            {
                var backups = LoadBackups();
                int restoredCount = 0;

                foreach (var entry in backups.Values)
                {
                    RegistryKey? baseKey = entry.Hive switch
                    {
                        "HKLM" => Registry.LocalMachine,
                        "HKCU" => Registry.CurrentUser,
                        _ => null
                    };

                    if (baseKey == null) continue;

                    using var subKey = baseKey.OpenSubKey(entry.SubKey, true);
                    if (subKey != null && entry.Value != null)
                    {
                        var kind = Enum.TryParse<RegistryValueKind>(entry.ValueKind, out var parsedKind)
                            ? parsedKind
                            : RegistryValueKind.DWord;

                        if (entry.Value is JsonElement element)
                        {
                            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int intVal))
                            {
                                subKey.SetValue(entry.ValueName, intVal, kind);
                            }
                            else if (element.ValueKind == JsonValueKind.String)
                            {
                                subKey.SetValue(entry.ValueName, element.GetString() ?? "", kind);
                            }
                        }
                        else
                        {
                            subKey.SetValue(entry.ValueName, entry.Value, kind);
                        }
                        restoredCount++;
                    }
                }

                // Restaurar plano de energia equilibrado
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "-setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit(5000);

                return (true, $"Restauradas com sucesso {restoredCount} configurações originais do Windows!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao restaurar: {ex.Message}");
            }
        }
    }
}
