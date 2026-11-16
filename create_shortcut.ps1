# Obter o caminho do Ambiente de Trabalho do utilizador atual
$desktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
if (-not $desktopPath -or -not (Test-Path $desktopPath)) {
    if (Test-Path (Join-Path $HOME "Desktop")) {
        $desktopPath = Join-Path $HOME "Desktop"
    } elseif (Test-Path (Join-Path $HOME "OneDrive\Desktop")) {
        $desktopPath = Join-Path $HOME "OneDrive\Desktop"
    } elseif (Test-Path (Join-Path $HOME "OneDrive\Área de Trabalho")) {
        $desktopPath = Join-Path $HOME "OneDrive\Área de Trabalho"
    } elseif (Test-Path (Join-Path $HOME "Área de Trabalho")) {
        $desktopPath = Join-Path $HOME "Área de Trabalho"
    }
}

$appDir = $PSScriptRoot
if (-not $appDir) { $appDir = (Get-Location).Path }

$targetExe = Join-Path $appDir "FPSBoostPro.exe"
if (-not (Test-Path $targetExe)) {
    $targetExe = Join-Path $appDir "bin\Release\net6.0-windows\win-x64\publish\FPSBoostPro.exe"
}
if (-not (Test-Path $targetExe)) {
    $targetExe = Join-Path $appDir "bin\Release\net6.0-windows\FPSBoostPro.exe"
}
if (-not (Test-Path $targetExe)) {
    $targetExe = Join-Path $appDir "bin\Debug\net6.0-windows\FPSBoostPro.exe"
}

$iconPath = Join-Path $appDir "app.ico"
if (-not (Test-Path $iconPath)) {
    $iconPath = $targetExe
}

$shortcutPath = Join-Path $desktopPath "FPSBoost Pro.lnk"

$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $appDir
$shortcut.IconLocation = "$iconPath,0"
$shortcut.Description = "FPSBoost Pro - Otimizador de Jogos (Rust, CS2, PUBG)"
$shortcut.Save()

try {
    # Set byte 0x15 bit 0x20 to run automatically as Administrator
    $bytes = [System.IO.File]::ReadAllBytes($shortcutPath)
    $bytes[0x15] = $bytes[0x15] -bor 0x20
    [System.IO.File]::WriteAllBytes($shortcutPath, $bytes)
} catch {
    # Non-fatal if byte modification fails
}

Write-Output "Atalho criado com sucesso no Ambiente de Trabalho: $shortcutPath"
