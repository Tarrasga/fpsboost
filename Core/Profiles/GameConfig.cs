using System;

namespace FPSBoostPro.Core.Profiles
{
    public class GameConfig
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string GameName { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public string Priority { get; set; } = "Alta"; // "Alta", "Acima do Normal", "Tempo Real"
        public bool EnableTimerResolution { get; set; } = true;
        public bool AutoCleanRamOnLaunch { get; set; } = true;
        public bool AutoCleanPeriodicRam { get; set; } = true;
        public int PeriodicMinutes { get; set; } = 15;
        public string LaunchOptions { get; set; } = "";
        public string ConsoleCommands { get; set; } = "";
        public string InGameTips { get; set; } = "";
    }

    public class GameSessionLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string GameName { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string DurationFormatted { get; set; } = "Em execução...";
        public double RamFreedMb { get; set; }
        public string PriorityApplied { get; set; } = "Alta";
        public string StatusColor { get; set; } = "#FF1E42";
        public string Details { get; set; } = "";

        public void CalculateDuration()
        {
            if (EndTime.HasValue)
            {
                var span = EndTime.Value - StartTime;
                if (span.TotalHours >= 1)
                {
                    DurationFormatted = $"{(int)span.TotalHours}h {span.Minutes}m";
                }
                else if (span.TotalMinutes >= 1)
                {
                    DurationFormatted = $"{(int)span.TotalMinutes} min {span.Seconds}s";
                }
                else
                {
                    DurationFormatted = $"{Math.Max(1, (int)span.TotalSeconds)} seg";
                }
            }
        }
    }
}
