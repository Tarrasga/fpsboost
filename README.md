# ⚡ FPSBoost Pro - Otimizador de FPS & Latência

Aplicação nativa Windows de alta performance criada especialmente para jogadores de **Rust**, **Counter-Strike 2 (CS2)** e **PUBG: BATTLEGROUNDS** (com suporte para qualquer outro jogo).

O **FPSBoost Pro** elimina as causas reais de perda de FPS e picos de ping no Windows: micro-stuttering, retenção ineficiente de memória standby, algoritmos de atraso em pacotes de rede (Nagle's Algorithm), throttling do Windows e gravação oculta em segundo plano.

---

## 🎯 Menus e Funcionalidades Principais

| Módulo | Descrição |
|---|---|
| **📊 Painel Geral (Dashboard)** | Monitor em tempo real de CPU, GPU, RAM e Plano de Energia; botão de **"Otimizar Agora (1-Clique)"**; status de jogo ativo. |
| **🚀 FPS & Sistema** | Ativação do **Desempenho Máximo** (`powercfg`), desativação de **Xbox Game DVR**, prioridade de processador 100% para jogos (`SystemResponsiveness = 0`), remoção de aceleração do rato (mira 1:1) e limpeza de shaders DirectX e temporários. |
| **🌐 Ping & Latência** | Desativação do **Algoritmo de Nagle** (`TcpAckFrequency = 1` e `TCPNoDelay = 1`), desbloqueio do limite de rede do Windows (`NetworkThrottlingIndex = 0xffffffff`), DNS de Baixa Latência (**Cloudflare 1.1.1.1** e **Google 8.8.8.8**), Flush DNS e teste de ping com jitter em tempo real. |
| **⚙️ Configs por Jogo** | **Menu de configuração individual por jogo**: Permite definir para cada jogo (Rust, CS2, PUBG ou novos jogos adicionados) o nível de prioridade da CPU, resolução de timer (1.0ms), limpeza de RAM inicial e periódica em segundo plano, opções de arranque da Steam e comandos de consola. Salva automaticamente em JSON! |
| **📜 Log dos Jogos (Sessões)** | **Histórico e telemetria de jogos**: Regista automaticamente cada sessão quando começas e terminas de jogar, mostrando a data/hora, tempo total de jogo, quantidade de memória RAM libertada (+MB) e otimizações aplicadas. |
| **🛡️ Segurança & Backup** | Criação de **Ponto de Restauração oficial do Windows** e botão para **Restaurar Tudo aos Padrões** (reverte 100% das alterações no registro). |

---

## 🎮 O que o aplicativo faz pelos teus jogos?

### 1. ⚡ Rust
* **Fim dos congelamentos por Garbage Collection**: Permite copiar e aplicar o comando `gc.buffer 2048` ou `gc.buffer 4096`, impedindo as temidas paragens de 1 segundo em tiroteios.
* **Auto-Trim de Memória Standby Periódico**: O Rust consome muita RAM e acumula cache no Windows. A aplicação limpa o cache periodicamente a cada 10-15 minutos em segundo plano enquanto jogas.
* **Launch Options Personalizadas**: Define alocação de memória baseada na tua RAM real detetada.

### 2. 🎯 Counter-Strike 2 (CS2)
* **Desativação do Algoritmo de Nagle (`TCPNoDelay` & `TcpAckFrequency = 1`)**: Faz com que cada pacote de sub-tick seja despachado instantaneamente pela placa de rede, garantindo que os tiros registam sem atrasos.
* **Temporizador de Alta Resolução (1.0ms)**: Sincronização de frames e resposta de rato mais consistentes.
* **Aceleração do Rato Desativada**: Garante sensibilidade de mira 1:1 absoluta.
* **Launch Options da Steam**: `-novid -high +fps_max 0 -nojoy`.

### 3. 🪂 PUBG: BATTLEGROUNDS
* **Fim do Network Throttling**: Por padrão, o Windows reduz a prioridade de dados de rede quando há multimídia ativa. O app define `NetworkThrottlingIndex = 0xffffffff` para manter o fluxo de pacotes UDP desimpedido.
* **Otimização de Prioridade de Processo**: Prioridade Alta automática quando o `TslGame.exe` é aberto.
* **Recomendações DirectX 11 Enhanced**: Menos quedas de framerate em cidades e tiroteios intensos.

---

## 🛠️ Como Executar

1. Abre a pasta `E:\fpsboost`.
2. Dá duplo-clique no ficheiro **`Iniciar_FPSBoost.bat`**.
3. O Windows pedirá confirmação de Administrador (necessário para aplicar alterações de registro e rede com segurança).
4. O app abre de imediato em modo Gamer Dark Theme.

Ou podes executar diretamente o executável compilado:
`bin\Release\net6.0-windows\FPSBoostPro.exe`
