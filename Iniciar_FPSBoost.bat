@echo off
title FPSBoost Pro - Iniciando...
cd /d "%~dp0"

echo ========================================================
echo         FPSBOOST PRO - MÁXIMO FPS E MENOR PING
echo ========================================================
echo.
echo A verificar permissoes de Administrador...

:: Verifica permissoes de Administrador e eleva se necessario
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [OK] Privilegios de Administrador confirmados.
    if exist "FPSBoostPro.exe" (
        start "" "FPSBoostPro.exe"
    ) else if exist "bin\Release\net6.0-windows\win-x64\publish\FPSBoostPro.exe" (
        start "" "bin\Release\net6.0-windows\win-x64\publish\FPSBoostPro.exe"
    ) else if exist "bin\Release\net6.0-windows\FPSBoostPro.exe" (
        start "" "bin\Release\net6.0-windows\FPSBoostPro.exe"
    ) else if exist "bin\Debug\net6.0-windows\FPSBoostPro.exe" (
        start "" "bin\Debug\net6.0-windows\FPSBoostPro.exe"
    ) else (
        echo [!] Executavel FPSBoostPro.exe nao encontrado nesta pasta.
        pause
    )
) else (
    echo [i] A solicitar elevacao de Administrador...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c \"\"%~f0\"\"' -Verb runas"
    exit /b
)
