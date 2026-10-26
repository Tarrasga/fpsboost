using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace FPSBoostPro.Core.Monitoring
{
    public class PingResult
    {
        public string HostName { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public long MinPing { get; set; }
        public long AvgPing { get; set; }
        public long MaxPing { get; set; }
        public double Jitter { get; set; }
        public int PacketLossPercent { get; set; }
        public bool Success { get; set; }
        public string StatusColor { get; set; } = "#00FFA3"; // Green, yellow or red
    }

    public class PingTester
    {
        public static async Task<PingResult> TestHostAsync(string hostName, string address, int packetCount = 4)
        {
            var pings = new List<long>();
            int lostPackets = 0;

            using var sender = new Ping();

            for (int i = 0; i < packetCount; i++)
            {
                try
                {
                    var reply = await sender.SendPingAsync(address, 1500);
                    if (reply.Status == IPStatus.Success)
                    {
                        pings.Add(reply.RoundtripTime);
                    }
                    else
                    {
                        lostPackets++;
                    }
                }
                catch
                {
                    lostPackets++;
                }

                if (i < packetCount - 1)
                {
                    await Task.Delay(120);
                }
            }

            if (pings.Count == 0)
            {
                return new PingResult
                {
                    HostName = hostName,
                    IpAddress = address,
                    Success = false,
                    PacketLossPercent = 100,
                    StatusColor = "#FF4B68"
                };
            }

            long min = pings.Min();
            long max = pings.Max();
            long avg = (long)pings.Average();

            // Cálculo do Jitter (média da diferença absoluta entre pings sucessivos)
            double jitter = 0;
            if (pings.Count > 1)
            {
                double totalDiff = 0;
                for (int i = 1; i < pings.Count; i++)
                {
                    totalDiff += Math.Abs(pings[i] - pings[i - 1]);
                }
                jitter = totalDiff / (pings.Count - 1);
            }

            int lossPercent = (int)((lostPackets / (double)packetCount) * 100.0);

            // Cor baseada na latência (Tema Red & Black)
            string color = avg switch
            {
                < 30 => "#FF1E42",  // Excelente (Crimson Neon)
                < 60 => "#FF6600",  // Muito Bom (Flame Orange)
                < 100 => "#FFAA00", // Razoável (Amber)
                _ => "#888888"      // Alto / Fraco
            };

            return new PingResult
            {
                HostName = hostName,
                IpAddress = address,
                MinPing = min,
                AvgPing = avg,
                MaxPing = max,
                Jitter = Math.Round(jitter, 1),
                PacketLossPercent = lossPercent,
                Success = true,
                StatusColor = color
            };
        }

        public static async Task<List<PingResult>> RunFullPingDiagnosticAsync(Action<string>? progressCallback = null)
        {
            var targets = new (string Name, string IP)[]
            {
                ("Cloudflare Gaming DNS", "1.1.1.1"),
                ("Google DNS Gaming", "8.8.8.8"),
                ("Servidores Valve CS2 (EU)", "155.133.226.1"),
                ("Servidores Rust (EU)", "162.254.197.1"),
                ("Servidores PUBG (AWS EU)", "3.120.0.0")
            };

            var results = new List<PingResult>();

            foreach (var target in targets)
            {
                progressCallback?.Invoke($"A testar ping para {target.Name}...");
                var res = await TestHostAsync(target.Name, target.IP);
                results.Add(res);
            }

            return results;
        }
    }
}
