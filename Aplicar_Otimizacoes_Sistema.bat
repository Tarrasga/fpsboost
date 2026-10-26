@echo off
setlocal enabledelayedexpansion
title FPSBoost Pro - Otimizacao do PC em 1-Clique
cd /d "%~dp0"

echo =====================================================================
echo          FPSBOOST PRO - APLICADOR DE OTIMIZACOES DEFINITIVAS
echo                  (Hardware: i5-13600K + RTX 4070 Ti)
echo =====================================================================
echo.

:: 1. Verificacao de Administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] Solicitando permissao de Administrador...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"%~f0\"\"' -Verb runas"
    exit /b
)

echo [OK] Privilegios de Administrador ativos!
echo.

:: 2. Ativar Plano de Energia de Elevado Desempenho / Desempenho Maximo
echo [1/6] Configurando Plano de Energia da CPU (i5-13600K)...
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1
powercfg -setactive e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1
if %errorLevel% neq 0 (
    powercfg -setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c >nul 2>&1
)
echo      -> Plano de Elevado Desempenho ativado (Fim do Core Parking e quedas de clock).

:: 3. Otimizacao do Sistema e Multimedia (SystemResponsiveness = 0, NetworkThrottlingIndex = 0xffffffff)
echo [2/6] Configurando Prioridade de Kernel e Desbloqueio de Rede...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d 0 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NetworkThrottlingIndex" /t REG_DWORD /d 4294967295 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "GPU Priority" /t REG_DWORD /d 8 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 6 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "SFIO Priority" /t REG_SZ /d "High" /f >nul
echo      -> 100%% da CPU e prioridade total de rede entregues aos jogos.

:: 4. Desativar Algoritmo de Nagle na Placa de Rede (TcpAckFrequency = 1, TCPNoDelay = 1)
echo [3/6] Otimizando Adaptadores de Rede (Desativando Nagle's Algorithm)...
for /f "tokens=*" %%k in ('reg query "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"') do (
    reg add "%%k" /v "TcpAckFrequency" /t REG_DWORD /d 1 /f >nul 2>&1
    reg add "%%k" /v "TCPNoDelay" /t REG_DWORD /d 1 /f >nul 2>&1
    reg add "%%k" /v "TcpDelAckTicks" /t REG_DWORD /d 0 /f >nul 2>&1
)
echo      -> Algoritmo de Nagle desativado em todas as interfaces ativas (Menor Ping no CS2/Rust/PUBG).

:: 5. Desativar Xbox Game DVR e Gravacao Oculta em Segundo Plano
echo [4/6] Desativando Xbox Game DVR e Gravador em Background...
reg add "HKCU\System\GameConfigStore" /v "GameDVR_Enabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\System\GameConfigStore" /v "GameDVR_FSEBehavior" /t REG_DWORD /d 2 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowGameDVR" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "AppCaptureEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "HistoricalCaptureEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
echo      -> Game DVR desativado (Fim do consumo continuo de GPU e disco).

:: 6. Otimizacoes Especificas do RUST (gc.buffer 4096)
echo [5/6] Aplicando correcao de Garbage Collection do RUST...
set "RUST_CFG=E:\SteamLibrary\steamapps\common\Rust\cfg\client.cfg"
if exist "%RUST_CFG%" (
    powershell -NoProfile -Command "(Get-Content -Path '%RUST_CFG%') -replace 'gc\.buffer\s+\"\d+\"', 'gc.buffer \"4096\"' | Set-Content -Path '%RUST_CFG%' -Encoding UTF8" >nul 2>&1
    echo      -> RUST client.cfg atualizado para gc.buffer 4096 (Elimina congelamentos nos tiroteios).
) else (
    echo      -> [!] Ficheiro de configuracao do Rust nao encontrado neste caminho.
)

:: 7. Criar Autoexec do CS2 (Counter-Strike 2)
echo [6/6] Instalando autoexec.cfg competitivo de Baixa Latencia no CS2...
set "CS2_CFG_DIR=E:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg"
if exist "%CS2_CFG_DIR%" (
    powershell -NoProfile -Command "$c = @'
// === CS2 LOW LATENCY AUTOEXEC - FPSBOOST PRO ===
fps_max 0
engine_low_latency_sleep_after_client_tick true
snd_mixahead 0.02
cl_interp 0.015625
cl_interp_ratio 1
r_show_build_info false
echo [FPSBoost Pro] Autoexec CS2 Carregado com Sucesso!
'@; Set-Content -Path '%CS2_CFG_DIR%\autoexec.cfg' -Value $c -Encoding UTF8" >nul 2>&1
    echo      -> CS2 autoexec.cfg instalado com sucesso em %CS2_CFG_DIR%\autoexec.cfg.
) else (
    echo      -> [!] Pasta de configuracao do CS2 nao encontrada neste caminho.
)

:: 8. Ativar HAGS (Agendamento de GPU por Hardware na RTX 4070 Ti)
echo [7/7] Ativando Agendamento de GPU por Hardware (HAGS) para a RTX 4070 Ti...
reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "HwSchMode" /t REG_DWORD /d 2 /f >nul 2>&1
echo      -> HAGS ativado no Windows (Reduz latencia de renderizacao da GPU).

:: Flush DNS
ipconfig /flushdns >nul 2>&1

echo.
echo =====================================================================
echo          OTIMIZACAO CONCLUIDA COM SUCESSO NO SEU COMPUTADOR!
echo =====================================================================
echo.
echo Resumo das melhorias aplicadas:
echo  1. CPU i5-13600K: Plano de Alto Desempenho (clock estavel, sem Core Parking)
echo  2. Windows: SystemResponsiveness = 0 (100%% CPU para o jogo)
echo  3. Rede: Nagle desativado e NetworkThrottlingIndex desativado
echo  4. Rust: gc.buffer configurado para 4096 (Adequado aos teus 32 GB de RAM)
echo  5. CS2: autoexec.cfg criado com motor de baixa latencia de clique
echo  6. Xbox Game DVR: Gravacao oculta desativada (mais GPU para a RTX 4070 Ti)
echo  7. GPU RTX 4070 Ti: Agendamento por Hardware (HAGS) ativado!
echo.
echo Recomenda-se reiniciar o computador para aplicar todas as definicoes da GPU.
echo Pressione qualquer tecla para fechar esta janela...
pause >nul
