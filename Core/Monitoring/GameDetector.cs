using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using FPSBoostPro.Core.Profiles;

namespace FPSBoostPro.Core.Monitoring
{
    public class GameDetector
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        private readonly GameConfigManager _configManager;
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _periodicRamTimer;

        private string? _currentlyBoostedGame;
        private string? _currentlyBoostedProcess;
        private int _currentProcessId;
        private bool _timerPeriodActive;

        public event Action<string, int>? OnGameStarted;
        public event Action<string>? OnGameExited;
        public event Action<string>? OnStatusMessage;

        public string? ActiveGameName => _currentlyBoostedGame;
        public bool IsGameRunning => !string.IsNullOrEmpty(_currentlyBoostedGame);

        public GameDetector(GameConfigManager configManager)
        {
            _configManager = configManager;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };
            _timer.Tick += (s, e) => CheckRunningGames();

            _periodicRamTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(10)
            };
            _periodicRamTimer.Tick += (s, e) =>
            {
                if (IsGameRunning)
                {
                    var res = Tweaks.MemoryOptimizer.OptimizeMemory();
                    OnStatusMessage?.Invoke($"[AUTO-RAM] Limpeza periódica em jogo: {res.MbFreed:F0} MB libertados!");
                }
            };
        }

        public void Start()
        {
            _timer.Start();
            CheckRunningGames();
        }

        public void Stop()
        {
            _timer.Stop();
            _periodicRamTimer.Stop();

            if (_timerPeriodActive)
            {
                TimeEndPeriod(1);
                _timerPeriodActive = false;
            }

            if (_currentlyBoostedGame != null)
            {
                _configManager.EndGameSession();
                _currentlyBoostedGame = null;
            }
        }

        private void CheckRunningGames()
        {
            try
            {
                // 1. Se já existe jogo monitorizado, verifica se ainda está em execução
                if (_currentlyBoostedGame != null)
                {
                    bool stillAlive = false;
                    try
                    {
                        var proc = Process.GetProcessById(_currentProcessId);
                        if (!proc.HasExited)
                        {
                            stillAlive = true;
                        }
                    }
                    catch
                    {
                        stillAlive = false;
                    }

                    if (!stillAlive)
                    {
                        string exitedGame = _currentlyBoostedGame;
                        _currentlyBoostedGame = null;
                        _currentlyBoostedProcess = null;
                        _currentProcessId = 0;
                        _periodicRamTimer.Stop();

                        if (_timerPeriodActive)
                        {
                            TimeEndPeriod(1);
                            _timerPeriodActive = false;
                        }

                        // Regista final da sessão no histórico
                        _configManager.EndGameSession();

                        OnGameExited?.Invoke(exitedGame);
                        OnStatusMessage?.Invoke($"Jogo fechado: {exitedGame}. Sessão gravada no histórico.");
                    }
                    return;
                }

                // 2. Procura processos entre todas as configurações salvas de jogos
                foreach (var config in _configManager.Configs)
                {
                    if (string.IsNullOrWhiteSpace(config.ProcessName)) continue;

                    string cleanName = config.ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                        ? config.ProcessName[..^4]
                        : config.ProcessName;

                    var matching = Process.GetProcessesByName(cleanName);
                    if (matching.Length > 0)
                    {
                        var target = matching[0];
                        _currentlyBoostedGame = config.GameName;
                        _currentlyBoostedProcess = cleanName;
                        _currentProcessId = target.Id;

                        // Aplica Boost personalizado com base na config do jogo
                        double freedMb = BoostGameProcess(target, config);

                        // Regista início da sessão
                        _configManager.StartGameSession(config.GameName, cleanName, config.Priority, freedMb);

                        if (config.AutoCleanPeriodicRam && config.PeriodicMinutes > 0)
                        {
                            _periodicRamTimer.Interval = TimeSpan.FromMinutes(config.PeriodicMinutes);
                            _periodicRamTimer.Start();
                        }

                        OnGameStarted?.Invoke(config.GameName, target.Id);
                        OnStatusMessage?.Invoke($"Jogo detetado: {config.GameName}! Configuração específica ({config.Priority}) aplicada.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao monitorizar jogos: {ex.Message}");
            }
        }

        private double BoostGameProcess(Process proc, GameConfig config)
        {
            double freedMb = 0;
            try
            {
                // 1. Elevar prioridade de CPU conforme a config
                var priorityClass = config.Priority switch
                {
                    "Tempo Real" => ProcessPriorityClass.RealTime,
                    "Acima do Normal" => ProcessPriorityClass.AboveNormal,
                    _ => ProcessPriorityClass.High
                };

                if (proc.PriorityClass != priorityClass)
                {
                    proc.PriorityClass = priorityClass;
                }

                // 2. Timer Resolution
                if (config.EnableTimerResolution && !_timerPeriodActive)
                {
                    TimeBeginPeriod(1);
                    _timerPeriodActive = true;
                }

                // 3. Limpeza preventiva de memória
                if (config.AutoCleanRamOnLaunch)
                {
                    var memRes = Tweaks.MemoryOptimizer.OptimizeMemory();
                    freedMb = memRes.MbFreed;
                }
            }
            catch
            {
                // Requer permissão de Administrador em processos protegidos
            }
            return freedMb;
        }
    }
}
