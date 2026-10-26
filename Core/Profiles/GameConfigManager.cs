using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FPSBoostPro.Core.Profiles
{
    public class GameConfigManager
    {
        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FPSBoostPro"
        );
        private static readonly string ConfigFile = Path.Combine(AppDataDir, "game_configs.json");
        private static readonly string SessionsFile = Path.Combine(AppDataDir, "game_sessions.json");

        private List<GameConfig> _configs = new();
        private List<GameSessionLog> _sessions = new();
        private GameSessionLog? _currentActiveSession;

        public IReadOnlyList<GameConfig> Configs => _configs;
        public IReadOnlyList<GameSessionLog> Sessions => _sessions;
        public GameSessionLog? CurrentActiveSession => _currentActiveSession;

        public event Action? OnSessionsUpdated;

        public GameConfigManager()
        {
            EnsureDirectories();
            LoadConfigs();
            LoadSessions();
        }

        private void EnsureDirectories()
        {
            try
            {
                if (!Directory.Exists(AppDataDir))
                {
                    Directory.CreateDirectory(AppDataDir);
                }
            }
            catch { }
        }

        #region Game Configurations

        public void LoadConfigs()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    string json = File.ReadAllText(ConfigFile);
                    var list = JsonSerializer.Deserialize<List<GameConfig>>(json);
                    if (list != null && list.Count > 0)
                    {
                        _configs = list;
                        return;
                    }
                }
            }
            catch { }

            // Se não existir, inicializa configurações padrão para Rust, CS2 e PUBG
            _configs = GetDefaultConfigs();
            SaveConfigs();
        }

        public void SaveConfigs()
        {
            try
            {
                EnsureDirectories();
                string json = JsonSerializer.Serialize(_configs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFile, json);
            }
            catch { }
        }

        public GameConfig? FindConfigByProcessName(string processName)
        {
            return _configs.FirstOrDefault(c =>
                c.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
                c.ProcessName.Equals($"{processName}.exe", StringComparison.OrdinalIgnoreCase) ||
                $"{c.ProcessName}.exe".Equals(processName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddOrUpdateConfig(GameConfig config)
        {
            int index = _configs.FindIndex(c => c.Id == config.Id);
            if (index >= 0)
            {
                _configs[index] = config;
            }
            else
            {
                _configs.Add(config);
            }
            SaveConfigs();
        }

        public void RemoveConfig(string id)
        {
            _configs.RemoveAll(c => c.Id == id);
            SaveConfigs();
        }

        private List<GameConfig> GetDefaultConfigs()
        {
            var mem = Tweaks.MemoryOptimizer.GetCurrentMemoryStats();
            int memMb = (int)(Math.Max(8, Math.Floor(mem.TotalGb)) * 1024);

            return new List<GameConfig>
            {
                new GameConfig
                {
                    Id = "rust",
                    GameName = "Rust",
                    ProcessName = "RustClient",
                    Priority = "Alta",
                    EnableTimerResolution = true,
                    AutoCleanRamOnLaunch = true,
                    AutoCleanPeriodicRam = true,
                    PeriodicMinutes = 10,
                    LaunchOptions = $"-high -maxMem={memMb} -malloc=system -cpuCount=8",
                    ConsoleCommands = "gc.buffer 2048",
                    InGameTips = "Pressiona F1 e escreve 'gc.buffer 2048' para impedir os congelamentos de tiroteio. Qualidade gráfica em 3 para descarregar a CPU."
                },
                new GameConfig
                {
                    Id = "cs2",
                    GameName = "Counter-Strike 2",
                    ProcessName = "cs2",
                    Priority = "Alta",
                    EnableTimerResolution = true,
                    AutoCleanRamOnLaunch = true,
                    AutoCleanPeriodicRam = false,
                    PeriodicMinutes = 20,
                    LaunchOptions = "-novid -high +fps_max 0 -nojoy",
                    ConsoleCommands = "fps_max 0; snd_mixahead 0.02",
                    InGameTips = "NVIDIA Reflex Ativado + Boost nas opções de vídeo. Nagle desativado para o registo sub-tick perfeito."
                },
                new GameConfig
                {
                    Id = "pubg",
                    GameName = "PUBG: BATTLEGROUNDS",
                    ProcessName = "TslGame",
                    Priority = "Alta",
                    EnableTimerResolution = true,
                    AutoCleanRamOnLaunch = true,
                    AutoCleanPeriodicRam = true,
                    PeriodicMinutes = 15,
                    LaunchOptions = "-USEALLAVAILABLECORES -malloc=system",
                    ConsoleCommands = "DirectX 11 Enhanced",
                    InGameTips = "Utiliza DirectX 11 Enhanced nas definições do jogo. Escala de renderização em 100% e sombras em Muito Baixo."
                }
            };
        }

        #endregion

        #region Session Logs

        public void LoadSessions()
        {
            try
            {
                if (File.Exists(SessionsFile))
                {
                    string json = File.ReadAllText(SessionsFile);
                    var list = JsonSerializer.Deserialize<List<GameSessionLog>>(json);
                    if (list != null)
                    {
                        _sessions = list;
                        return;
                    }
                }
            }
            catch { }
            _sessions = new List<GameSessionLog>();
        }

        public void SaveSessions()
        {
            try
            {
                EnsureDirectories();
                string json = JsonSerializer.Serialize(_sessions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SessionsFile, json);
            }
            catch { }
        }

        public GameSessionLog StartGameSession(string gameName, string processName, string priority, double ramFreedMb)
        {
            var session = new GameSessionLog
            {
                GameName = gameName,
                ProcessName = processName,
                StartTime = DateTime.Now,
                PriorityApplied = priority,
                RamFreedMb = ramFreedMb,
                StatusColor = "#FF1E42",
                Details = $"Prioridade {priority} + Timer 1.0ms + {ramFreedMb:F0}MB RAM libertada"
            };

            _currentActiveSession = session;
            _sessions.Insert(0, session);

            // Mantém no máximo as últimas 50 sessões
            if (_sessions.Count > 50)
            {
                _sessions = _sessions.Take(50).ToList();
            }

            SaveSessions();
            OnSessionsUpdated?.Invoke();
            return session;
        }

        public void EndGameSession()
        {
            if (_currentActiveSession != null)
            {
                _currentActiveSession.EndTime = DateTime.Now;
                _currentActiveSession.CalculateDuration();
                _currentActiveSession.StatusColor = "#94A3B8";
                _currentActiveSession = null;

                SaveSessions();
                OnSessionsUpdated?.Invoke();
            }
        }

        public void ClearSessions()
        {
            _sessions.Clear();
            _currentActiveSession = null;
            SaveSessions();
            OnSessionsUpdated?.Invoke();
        }

        #endregion
    }
}
