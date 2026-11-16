using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FPSBoostPro.Core.Monitoring;
using FPSBoostPro.Core.Profiles;
using FPSBoostPro.Core.Safety;
using FPSBoostPro.Core.Tweaks;

namespace FPSBoostPro
{
    public partial class MainWindow : Window
    {
        private readonly GameConfigManager _configManager;
        private readonly GameDetector _gameDetector;
        private readonly DispatcherTimer _statsTimer;
        private GameConfig? _selectedConfig;

        public MainWindow()
        {
            InitializeComponent();

            _configManager = new GameConfigManager();
            _configManager.OnSessionsUpdated += () => Dispatcher.Invoke(UpdateSessionsUi);

            _gameDetector = new GameDetector(_configManager);
            _gameDetector.OnGameStarted += OnGameStarted;
            _gameDetector.OnGameExited += OnGameExited;
            _gameDetector.OnStatusMessage += msg => LogMessage(msg);

            _statsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _statsTimer.Tick += (s, e) => UpdateMemoryStats();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Verifica privilégios de Administrador
                bool isAdmin = SystemInfoProvider.CheckIsAdministrator();
                AdminBanner.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;

                // 2. Carrega especificações do computador
                var specs = SystemInfoProvider.GetSystemSpecifications();
                TxtSidebarCpu.Text = specs.CpuName;
                TxtSidebarGpu.Text = specs.GpuName;
                TxtDashPowerPlan.Text = specs.ActivePowerPlan;
                TxtHeroHardware.Text = $"Hardware Detetado: {specs.CpuName} • {specs.GpuName} • {specs.RamInfo.Split('(')[0].Trim()} RAM";

                // 3. Atualiza memória RAM
                UpdateMemoryStats();

                // 4. Carrega seletor de jogos e histórico de sessões
                PopulateGameSelector();
                UpdateSessionsUi();

                // 5. Inicia detector automático de jogos
                _gameDetector.Start();
                _statsTimer.Start();

                LogMessage("FPSBoost Pro inicializado com sucesso.");
                if (!isAdmin)
                {
                    LogMessage("Aviso: A correr como Utilizador Comum. Para aplicar alterações de registro e rede no Windows, reinicie como Administrador.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erro na inicialização: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _gameDetector.Stop();
            _statsTimer.Stop();
        }

        #region Monitoring & Events

        private void UpdateMemoryStats()
        {
            try
            {
                var stats = MemoryOptimizer.GetCurrentMemoryStats();
                TxtDashRamUsage.Text = $"{stats.UsedGb:F1} GB / {stats.TotalGb:F1} GB ({stats.UsedPercentage}%)";
                ProgressRam.Value = stats.UsedPercentage;
                TxtStatusMemorySummary.Text = $"RAM: {stats.UsedGb:F1} / {stats.TotalGb:F1} GB ({stats.UsedPercentage}%)";

                if (stats.UsedPercentage > 85)
                {
                    ProgressRam.Foreground = (SolidColorBrush)FindResource("AccentRed");
                }
                else if (stats.UsedPercentage > 70)
                {
                    ProgressRam.Foreground = (SolidColorBrush)FindResource("AccentYellow");
                }
                else
                {
                    ProgressRam.Foreground = (SolidColorBrush)FindResource("AccentGreen");
                }
            }
            catch { }
        }

        private void OnGameStarted(string gameName, int pid)
        {
            Dispatcher.Invoke(() =>
            {
                GameStatusLed.Fill = (SolidColorBrush)FindResource("AccentGreen");
                TxtHeaderGameStatus.Text = $"🎮 {gameName} ATIVO & OTIMIZADO";
                TxtHeaderGameStatus.Foreground = (SolidColorBrush)FindResource("AccentGreen");
                TxtActiveSessionSummary.Text = $"🎮 {gameName} em execução";
                LogMessage($"[AUTO-BOOST] {gameName} (PID: {pid}) detetado! Configurações aplicadas.");
                UpdateSessionsUi();
            });
        }

        private void OnGameExited(string gameName)
        {
            Dispatcher.Invoke(() =>
            {
                GameStatusLed.Fill = (SolidColorBrush)FindResource("TextMuted");
                TxtHeaderGameStatus.Text = "Nenhum jogo detetado";
                TxtHeaderGameStatus.Foreground = (SolidColorBrush)FindResource("TextMuted");
                TxtActiveSessionSummary.Text = "Nenhum jogo ativo";
                LogMessage($"[MONITOR] {gameName} encerrado. Sessão registada no histórico.");
                UpdateSessionsUi();
            });
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string logLine = $"[{timestamp}] {message}\r\n";
                TxtLogConsole.AppendText(logLine);
                TxtLogConsole.ScrollToEnd();
                TxtStatusBarMessage.Text = message;
            });
        }

        #endregion

        #region Navigation Tabs

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelDashboard == null || PanelFpsSystem == null || PanelPingNetwork == null ||
                PanelGameConfigs == null || PanelGameSessions == null || PanelSafetyLogs == null)
            {
                return;
            }

            PanelDashboard.Visibility = Visibility.Collapsed;
            PanelFpsSystem.Visibility = Visibility.Collapsed;
            PanelPingNetwork.Visibility = Visibility.Collapsed;
            PanelGameConfigs.Visibility = Visibility.Collapsed;
            PanelGameSessions.Visibility = Visibility.Collapsed;
            PanelSafetyLogs.Visibility = Visibility.Collapsed;

            if (NavDashboard.IsChecked == true) PanelDashboard.Visibility = Visibility.Visible;
            else if (NavFpsSystem.IsChecked == true) PanelFpsSystem.Visibility = Visibility.Visible;
            else if (NavPingNetwork.IsChecked == true) PanelPingNetwork.Visibility = Visibility.Visible;
            else if (NavGameConfigs.IsChecked == true) PanelGameConfigs.Visibility = Visibility.Visible;
            else if (NavGameSessions.IsChecked == true)
            {
                UpdateSessionsUi();
                PanelGameSessions.Visibility = Visibility.Visible;
            }
            else if (NavSafetyLogs.IsChecked == true) PanelSafetyLogs.Visibility = Visibility.Visible;
        }

        #endregion

        #region Dashboard Actions

        private async void BtnQuickBoost_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("=== A INICIAR OTIMIZAÇÃO RÁPIDA (1-CLIQUE) ===");

            // 1. Desempenho Máximo
            var pwr = SystemTweaks.EnableUltimatePerformance();
            LogMessage(pwr.Message);
            TxtDashPowerPlan.Text = "Desempenho Máximo";

            // 2. Desativar GameDVR
            var dvr = SystemTweaks.DisableGameDVR();
            LogMessage(dvr.Message);

            // 3. Otimizar Prioridade de Sistema
            var resp = SystemTweaks.OptimizeSystemResponsiveness();
            LogMessage(resp.Message);

            // 4. HAGS & Prioridade GPU
            var hags = SystemTweaks.EnableHagsAndGpuOptimization();
            LogMessage(hags.Message);

            // 5. Desativar Nagle
            var nagle = NetworkTweaks.DisableNaglesAlgorithm();
            LogMessage(nagle.Message);

            // 6. Limpar DNS
            var dns = NetworkTweaks.FlushDns();
            LogMessage(dns.Message);

            // 7. Limpeza de Memória
            var mem = MemoryOptimizer.OptimizeMemory();
            LogMessage(mem.Message);
            UpdateMemoryStats();

            // 8. Teste de ping rápido
            var pingRes = await PingTester.TestHostAsync("Cloudflare", "1.1.1.1", 2);
            if (pingRes.Success)
            {
                TxtDashPing.Text = $"{pingRes.AvgPing} ms";
                TxtDashPing.Foreground = new BrushConverter().ConvertFromString(pingRes.StatusColor) as Brush;
            }

            LogMessage("=== OTIMIZAÇÃO CONCLUÍDA! PC PRONTO PARA MÁXIMO FPS E MENOR PING ===");
            MessageBox.Show("Otimização concluída com sucesso!\n\n• Desempenho Máximo Ativado\n• Agendamento de GPU (HAGS) Ativado\n• Algoritmo de Nagle Desativado\n• Prioridade Multimídia 100% para Jogos\n• Memória Standby e Cache Limpos", "FPSBoost Pro", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCleanRam_Click(object sender, RoutedEventArgs e)
        {
            var res = MemoryOptimizer.OptimizeMemory();
            UpdateMemoryStats();
            LogMessage(res.Message);
        }

        private void BtnEnablePowerPlan_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.EnableUltimatePerformance();
            TxtDashPowerPlan.Text = "Desempenho Máximo";
            LogMessage(res.Message);
        }

        private void BtnApplyHardwareTweaks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string batPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aplicar_Otimizacoes_Sistema.bat");
                if (!System.IO.File.Exists(batPath))
                {
                    string parentCandidate = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Aplicar_Otimizacoes_Sistema.bat"));
                    if (System.IO.File.Exists(parentCandidate))
                    {
                        batPath = parentCandidate;
                    }
                }

                if (System.IO.File.Exists(batPath))
                {
                    LogMessage("A executar Aplicar_Otimizacoes_Sistema.bat com privilégios de Administrador...");
                    var psi = new ProcessStartInfo
                    {
                        FileName = batPath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                    TxtDashPowerPlan.Text = "Desempenho Máximo";
                }
                else
                {
                    LogMessage("Script Aplicar_Otimizacoes_Sistema.bat não encontrado.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao executar otimizações diretas: {ex.Message}");
            }
        }

        #endregion

        #region FPS & System Tweaks

        private void BtnDisableGameDvr_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.DisableGameDVR();
            LogMessage(res.Message);
        }

        private void BtnSystemResponsiveness_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.OptimizeSystemResponsiveness();
            LogMessage(res.Message);
        }

        private void BtnDisableMouseAccel_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.DisableMouseAcceleration();
            LogMessage(res.Message);
        }

        private void BtnCleanShaderCache_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.CleanJunkAndShaderCache();
            LogMessage(res.Message);
        }

        private void BtnEnableHags_Click(object sender, RoutedEventArgs e)
        {
            var res = SystemTweaks.EnableHagsAndGpuOptimization();
            LogMessage(res.Message);
            if (res.Success)
            {
                MessageBox.Show(res.Message, "FPSBoost Pro - Otimização GPU", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(res.Message, "FPSBoost Pro - Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Network & Ping Tweaks

        private void BtnDisableNagles_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.DisableNaglesAlgorithm();
            LogMessage(res.Message);
        }

        private void BtnDnsCloudflare_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.SetGamingDns("1.1.1.1", "1.0.0.1");
            LogMessage(res.Message);
        }

        private void BtnDnsGoogle_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.SetGamingDns("8.8.8.8", "8.8.4.4");
            LogMessage(res.Message);
        }

        private void BtnDnsDhcp_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.SetGamingDns("", "");
            LogMessage(res.Message);
        }

        private void BtnFlushDns_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.FlushDns();
            LogMessage(res.Message);
        }

        private void BtnResetWinsock_Click(object sender, RoutedEventArgs e)
        {
            var res = NetworkTweaks.ResetWinsockAndTcpStack();
            LogMessage(res.Message);
            MessageBox.Show("Pilha TCP/IP e Winsock redefinidas.\nRecomenda-se reiniciar o computador para aplicar todas as rotas de rede.", "FPSBoost Pro", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnTestPing_Click(object sender, RoutedEventArgs e)
        {
            TxtPingStatusNotice.Text = "A executar diagnóstico de latência para servidores de jogo...";
            ListPingResults.ItemsSource = null;

            var results = await PingTester.RunFullPingDiagnosticAsync(step =>
            {
                TxtPingStatusNotice.Text = step;
            });

            ListPingResults.ItemsSource = results;
            TxtPingStatusNotice.Text = "Diagnóstico finalizado. Veja os resultados de ping médio e estabilidade (jitter) abaixo.";

            if (results.Count > 0 && results[0].Success)
            {
                TxtDashPing.Text = $"{results[0].AvgPing} ms";
                TxtDashPing.Foreground = new BrushConverter().ConvertFromString(results[0].StatusColor) as Brush;
            }

            LogMessage($"Diagnóstico de rede concluído para {results.Count} destinos.");
        }

        #endregion

        #region Game Configurations Editor

        private void PopulateGameSelector()
        {
            CmbGameSelector.Items.Clear();
            foreach (var cfg in _configManager.Configs)
            {
                CmbGameSelector.Items.Add(cfg.GameName);
            }

            if (CmbGameSelector.Items.Count > 0)
            {
                CmbGameSelector.SelectedIndex = 0;
            }
        }

        private void CmbGameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGameSelector.SelectedIndex >= 0 && CmbGameSelector.SelectedIndex < _configManager.Configs.Count)
            {
                _selectedConfig = _configManager.Configs[CmbGameSelector.SelectedIndex];
                LoadConfigIntoForm(_selectedConfig);
            }
        }

        private void LoadConfigIntoForm(GameConfig config)
        {
            TxtCfgGameName.Text = config.GameName;
            TxtCfgProcessName.Text = config.ProcessName;

            // Prioridade
            foreach (ComboBoxItem item in CmbCfgPriority.Items)
            {
                if (item.Content.ToString() == config.Priority)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            ChkCfgTimer.IsChecked = config.EnableTimerResolution;
            ChkCfgRamLaunch.IsChecked = config.AutoCleanRamOnLaunch;
            ChkCfgPeriodicRam.IsChecked = config.AutoCleanPeriodicRam;
            TxtCfgPeriodicMinutes.Text = config.PeriodicMinutes.ToString();
            TxtCfgLaunchOptions.Text = config.LaunchOptions;
            TxtCfgConsoleCommands.Text = config.ConsoleCommands;
            TxtCfgTips.Text = config.InGameTips;
        }

        private void BtnSaveGameConfig_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConfig == null) return;

            _selectedConfig.GameName = TxtCfgGameName.Text.Trim();
            _selectedConfig.ProcessName = TxtCfgProcessName.Text.Trim();
            _selectedConfig.Priority = (CmbCfgPriority.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Alta";
            _selectedConfig.EnableTimerResolution = ChkCfgTimer.IsChecked == true;
            _selectedConfig.AutoCleanRamOnLaunch = ChkCfgRamLaunch.IsChecked == true;
            _selectedConfig.AutoCleanPeriodicRam = ChkCfgPeriodicRam.IsChecked == true;

            if (int.TryParse(TxtCfgPeriodicMinutes.Text, out int mins))
            {
                _selectedConfig.PeriodicMinutes = Math.Max(1, mins);
            }

            _selectedConfig.LaunchOptions = TxtCfgLaunchOptions.Text.Trim();
            _selectedConfig.ConsoleCommands = TxtCfgConsoleCommands.Text.Trim();
            _selectedConfig.InGameTips = TxtCfgTips.Text.Trim();

            _configManager.AddOrUpdateConfig(_selectedConfig);
            LogMessage($"Configuração do jogo '{_selectedConfig.GameName}' guardada com sucesso!");
            MessageBox.Show($"Configuração de '{_selectedConfig.GameName}' guardada com sucesso!", "FPSBoost Pro", MessageBoxButton.OK, MessageBoxImage.Information);

            // Atualiza nomes no seletor
            int currIdx = CmbGameSelector.SelectedIndex;
            PopulateGameSelector();
            CmbGameSelector.SelectedIndex = currIdx;
        }

        private void BtnAddNewGame_Click(object sender, RoutedEventArgs e)
        {
            var newCfg = new GameConfig
            {
                GameName = "Novo Jogo",
                ProcessName = "jogo",
                Priority = "Alta",
                EnableTimerResolution = true,
                AutoCleanRamOnLaunch = true,
                AutoCleanPeriodicRam = false,
                PeriodicMinutes = 15,
                LaunchOptions = "-high",
                ConsoleCommands = "",
                InGameTips = "Configurações personalizadas para este jogo."
            };

            _configManager.AddOrUpdateConfig(newCfg);
            PopulateGameSelector();
            CmbGameSelector.SelectedIndex = CmbGameSelector.Items.Count - 1;
            LogMessage("Novo perfil de jogo criado. Ajuste o nome do executável e parâmetros.");
        }

        private void BtnDeleteGame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedConfig == null) return;

            var res = MessageBox.Show(
                $"Tens a certeza de que desejas eliminar a configuração de '{_selectedConfig.GameName}'?",
                "Eliminar Jogo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (res == MessageBoxResult.Yes)
            {
                _configManager.RemoveConfig(_selectedConfig.Id);
                LogMessage($"Configuração de '{_selectedConfig.GameName}' removida.");
                PopulateGameSelector();
            }
        }

        private void BtnCopyLaunch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtCfgLaunchOptions.Text);
                LogMessage("Opções de inicialização copiadas para a área de transferência!");
                MessageBox.Show($"Opções de inicialização copiadas:\n\n{TxtCfgLaunchOptions.Text}\n\nCole nas propriedades do jogo na Steam.", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao copiar: {ex.Message}");
            }
        }

        private void BtnCopyConsole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtCfgConsoleCommands.Text);
                LogMessage("Comando de consola copiado!");
                MessageBox.Show($"Comando copiado:\n\n{TxtCfgConsoleCommands.Text}\n\nAbra a consola dentro do jogo e cole com Ctrl+V.", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao copiar: {ex.Message}");
            }
        }

        #endregion

        #region Game Sessions History

        private void UpdateSessionsUi()
        {
            try
            {
                ListGameSessions.ItemsSource = null;
                ListGameSessions.ItemsSource = _configManager.Sessions;

                TxtTotalSessions.Text = _configManager.Sessions.Count.ToString();
                double totalRam = _configManager.Sessions.Sum(s => s.RamFreedMb);
                TxtTotalRamFreed.Text = totalRam >= 1024
                    ? $"{totalRam / 1024.0:F1} GB"
                    : $"{totalRam:F0} MB";

                TxtActiveSessionSummary.Text = _gameDetector.IsGameRunning
                    ? $"🎮 {_gameDetector.ActiveGameName} a correr"
                    : "Nenhum jogo ativo";
            }
            catch { }
        }

        private void BtnClearSessions_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Desejas limpar todo o histórico de sessões de jogos?",
                "Limpar Histórico",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (res == MessageBoxResult.Yes)
            {
                _configManager.ClearSessions();
                UpdateSessionsUi();
                LogMessage("Histórico de sessões de jogo foi limpo.");
            }
        }

        #endregion

        #region Safety & Backup

        private void BtnCreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("A solicitar criação de Ponto de Restauração ao Windows...");
            var res = BackupManager.CreateSystemRestorePoint();
            LogMessage(res.Message);
            MessageBox.Show(res.Message, "Ponto de Restauração", MessageBoxButton.OK, res.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void BtnRestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Tem a certeza de que deseja restaurar as configurações originais do Windows?\n\nIsto irá repor o plano de energia original e reverter as entradas do Registro.",
                "Confirmar Restauração",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirm == MessageBoxResult.Yes)
            {
                var res = BackupManager.RestoreOriginalDefaults();
                LogMessage(res.Message);
                MessageBox.Show(res.Message, "Restauração Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            TxtLogConsole.Clear();
            LogMessage("Histórico de logs limpo.");
        }

        private void BtnRestartAdmin_Click(object sender, RoutedEventArgs e)
        {
            SystemInfoProvider.RestartAsAdministrator();
        }

        #endregion
    }
}
