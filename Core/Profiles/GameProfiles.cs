using System;
using System.Collections.Generic;

namespace FPSBoostPro.Core.Profiles
{
    public class GameProfile
    {
        public string GameName { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public string BannerIcon { get; set; } = "";
        public string LaunchOptions { get; set; } = "";
        public string ConsoleTweak { get; set; } = "";
        public List<string> CriticalTips { get; set; } = new();
        public List<string> InGameSettings { get; set; } = new();
    }

    public class GameProfiles
    {
        public static GameProfile GetRustProfile()
        {
            var mem = Tweaks.MemoryOptimizer.GetCurrentMemoryStats();
            int memMb = (int)(Math.Max(8, Math.Floor(mem.TotalGb)) * 1024);

            return new GameProfile
            {
                GameName = "Rust",
                ProcessName = "RustClient",
                BannerIcon = "⚡",
                LaunchOptions = $"-high -maxMem={memMb} -malloc=system -cpuCount=8",
                ConsoleTweak = "gc.buffer 2048",
                CriticalTips = new List<string>
                {
                    "Unity GC Freeze: Pressiona F1 na consola do Rust e escreve 'gc.buffer 2048' ou 'gc.buffer 4096'. Isto elimina as paragens de 1 segundo nos tiroteios!",
                    "Standby Memory: Rust tem fugas de memória graves. O nosso Otimizador de RAM em segundo plano limpa o cache automaticamente.",
                    "Configuração Gráfica: Em Rust, colocar tudo no 'Mínimo' sobrecarrega a CPU. Usa Graphics Quality no 3 ou 4 para a GPU trabalhar mais."
                },
                InGameSettings = new List<string>
                {
                    "Graphics Quality: 3 ou 4 (transfere carga para a GPU)",
                    "Shadow Cascades: No Cascades (ganho de até 15 FPS)",
                    "Max Tree Meshes: 50 a 70",
                    "Water Reflections / Quality: 0 (Off)",
                    "Draw Distance: 1500 (suficiente para ver jogadores)",
                    "Anisotropic Filtering: 1 (ou Off)"
                }
            };
        }

        public static GameProfile GetCS2Profile()
        {
            return new GameProfile
            {
                GameName = "Counter-Strike 2",
                ProcessName = "cs2",
                BannerIcon = "🎯",
                LaunchOptions = "-novid -high +fps_max 0 -nojoy",
                ConsoleTweak = "fps_max 0; snd_mixahead 0.02",
                CriticalTips = new List<string>
                {
                    "Nagle's Algorithm: Desativar o algoritmo de Nagle (na aba Ping & Rede) faz com que o sub-tick do CS2 registe tiros instantaneamente.",
                    "NVIDIA Reflex: Define nas opções de vídeo como 'Enabled + Boost' para eliminar a fila de renderização e reduzir o atraso de clique.",
                    "Aceleração do Rato: Confirma na aba FPS & Sistema que a aceleração do rato do Windows está desativada para manter a mira muscular 1:1."
                },
                InGameSettings = new List<string>
                {
                    "Qualidade Global das Sombras: Baixo ou Médio (Medium mantém sombras de jogadores visíveis)",
                    "Detalhe de Modelos/Texturas: Baixo / Médio",
                    "Detalhe de Partículas: Baixo (impede quedas perto de fumo/granadas)",
                    "Oclusão de Ambiente: Desativado",
                    "FidelityFX Super Resolution (FSR): Desativado (Qualidade Máxima Nativa)",
                    "Modo de Baixa Latência NVIDIA Reflex: Ativado + Boost"
                }
            };
        }

        public static GameProfile GetPubgProfile()
        {
            return new GameProfile
            {
                GameName = "PUBG: BATTLEGROUNDS",
                ProcessName = "TslGame",
                BannerIcon = "🪂",
                LaunchOptions = "-USEALLAVAILABLECORES -malloc=system",
                ConsoleTweak = "Config DirectX 11 Enhanced",
                CriticalTips = new List<string>
                {
                    "DirectX 11 Enhanced: No PUBG, o modo DirectX 11 Enhanced oferece frametimes mais consistentes e menos micro-stutter do que o DX12.",
                    "Network Throttling: O PUBG envia pacotes UDP constantes. O ajuste de 'NetworkThrottlingIndex = 0xffffffff' previne que o Windows limite os dados da partida.",
                    "Render Scale: Mantém a escala de renderização em 100%. Valores superiores a 100% aumentam muito a latência de input."
                },
                InGameSettings = new List<string>
                {
                    "Escala de Renderização: 100",
                    "Anti-Aliasing: Alto (essencial para detetar inimigos à distância)",
                    "Pós-Processamento: Muito Baixo",
                    "Sombras: Muito Baixo (ganho brutal de FPS em cidades)",
                    "Texturas: Médio (balanço perfeito entre VRAM e visibilidade)",
                    "Efeitos e Folhagem: Muito Baixo (elimina sombras e vegetação excessiva)",
                    "Distância de Visão: Médio (jogadores são renderizados à mesma distância)"
                }
            };
        }
    }
}
