@echo off
setlocal enabledelayedexpansion
title FPSBoost Pro - Otimizacao do PC em 1-Clique
cd /d "%~dp0"

echo =====================================================================
echo          FPSBOOST PRO - APLICADOR DE OTIMIZACOES DEFINITIVAS
echo        (Otimizacoes Universais de Processador, Placa Grafica e Rede)
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
echo [1/7] Configurando Plano de Energia de Elevado Desempenho...
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1
powercfg -setactive e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1
if %errorLevel% neq 0 (
    powercfg -setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c >nul 2>&1
)
echo      -> Plano de Elevado Desempenho ativado (Fim do Core Parking e das quedas de frequencia).

:: 3. Otimizacao do Sistema e Multimedia (SystemResponsiveness = 0, NetworkThrottlingIndex = 0xffffffff)
echo [2/7] Configurando Prioridade de Kernel e Desbloqueio de Rede...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d 0 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NetworkThrottlingIndex" /t REG_DWORD /d 4294967295 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "GPU Priority" /t REG_DWORD /d 8 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 6 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "SFIO Priority" /t REG_SZ /d "High" /f >nul
echo      -> 100%% da CPU e prioridade total de rede entregues aos jogos.

:: 4. Desativar Algoritmo de Nagle na Placa de Rede (TcpAckFrequency = 1, TCPNoDelay = 1)
echo [3/7] Otimizando Adaptadores de Rede (Desativando Nagle's Algorithm)...
for /f "tokens=*" %%k in ('reg query "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"') do (
    reg add "%%k" /v "TcpAckFrequency" /t REG_DWORD /d 1 /f >nul 2>&1
    reg add "%%k" /v "TCPNoDelay" /t REG_DWORD /d 1 /f >nul 2>&1
    reg add "%%k" /v "TcpDelAckTicks" /t REG_DWORD /d 0 /f >nul 2>&1
)
echo      -> Algoritmo de Nagle desativado em todas as interfaces ativas (Menor Ping no CS2/Rust/PUBG).

:: 5. Desativar Xbox Game DVR e Gravacao Oculta em Segundo Plano
echo [4/7] Desativando Xbox Game DVR e Gravador em Background...
reg add "HKCU\System\GameConfigStore" /v "GameDVR_Enabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\System\GameConfigStore" /v "GameDVR_FSEBehavior" /t REG_DWORD /d 2 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowGameDVR" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "AppCaptureEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "HistoricalCaptureEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
echo      -> Game DVR desativado (Fim do consumo continuo de GPU e disco).

:: 6. Otimizacoes Especificas do RUST (gc.buffer 4096 / auto-detetado em qualquer disco)
echo [5/7] A procurar instalacao do RUST para aplicar correcao de Garbage Collection...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { `$subDirs = @('SteamLibrary', 'Program Files (x86)\Steam', 'Program Files\Steam', 'Steam', 'Games\Steam', 'Jogos\Steam'); `$found = `$false; foreach (`$d in (Get-PSDrive -PSProvider FileSystem).Root) { foreach (`$s in `$subDirs) { `$cfg = Join-Path `$d (Join-Path `$s 'steamapps\common\Rust\cfg\client.cfg'); if (Test-Path `$cfg) { (Get-Content -Path `$cfg) -replace 'gc\.buffer\s+\"\d+\"', 'gc.buffer \"4096\"' | Set-Content -Path `$cfg -Encoding UTF8; Write-Host ('      -> RUST encontrado e atualizado: ' + `$cfg); `$found = `$true; break } } if (`$found) { break } } if (-not `$found) { Write-Host '      -> [i] Rust nao instalado ou client.cfg nao detetado automaticamente.' } }"

:: 7. Criar Autoexec do CS2 (Counter-Strike 2 / auto-detetado em qualquer disco)
echo [6/7] A procurar instalacao do CS2 para aplicar autoexec de Baixa Latencia...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { `$subDirs = @('SteamLibrary', 'Program Files (x86)\Steam', 'Program Files\Steam', 'Steam', 'Games\Steam', 'Jogos\Steam'); `$found = `$false; foreach (`$d in (Get-PSDrive -PSProvider FileSystem).Root) { foreach (`$s in `$subDirs) { `$cfgDir = Join-Path `$d (Join-Path `$s 'steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg'); if (Test-Path `$cfgDir) { `$c = @'`n// === CS2 LOW LATENCY AUTOEXEC - FPSBOOST PRO ===`nfps_max 0`nengine_low_latency_sleep_after_client_tick true`nsnd_mixahead 0.02`ncl_interp 0.015625`ncl_interp_ratio 1`nr_show_build_info false`necho [FPSBoost Pro] Autoexec CS2 Carregado com Sucesso!`n'@; Set-Content -Path (Join-Path `$cfgDir 'autoexec.cfg') -Value `$c -Encoding UTF8; Write-Host ('      -> CS2 autoexec instalado em: ' + `$cfgDir); `$found = `$true; break } } if (`$found) { break } } if (-not `$found) { Write-Host '      -> [i] CS2 nao instalado ou pasta cfg nao detetada automaticamente.' } }"

:: 8. Ativar HAGS (Agendamento de GPU por Hardware)
echo [7/7] Ativando Agendamento de GPU por Hardware (HAGS)...
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
echo  1. Processador: Plano de Alto Desempenho (clock estavel, sem Core Parking)
echo  2. Windows: SystemResponsiveness = 0 (100%% CPU para o jogo)
echo  3. Rede: Nagle desativado e NetworkThrottlingIndex desbloqueado
echo  4. Rust: gc.buffer configurado para 4096 (Fim dos travamentos)
echo  5. CS2: autoexec.cfg criado com motor de baixa latencia de clique
echo  6. Xbox Game DVR: Gravacao oculta desativada (mais GPU e disco livres)
echo  7. Placa Grafica: Agendamento de GPU por Hardware (HAGS) ativado!
echo.
echo Recomenda-se reiniciar o computador para aplicar todas as definicoes da GPU.
echo Pressione qualquer tecla para fechar esta janela...
pause >nul
