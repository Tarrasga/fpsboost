$desktopPath = [Environment]::GetFolderPath('Desktop')
if (-not $desktopPath -or -not (Test-Path $desktopPath)) {
    if (Test-Path "C:\Users\cac_7\Desktop") {
        $desktopPath = "C:\Users\cac_7\Desktop"
    } elseif (Test-Path "C:\Users\cac_7\OneDrive\Desktop") {
        $desktopPath = "C:\Users\cac_7\OneDrive\Desktop"
    }
}

$targetExe = "E:\fpsboost\bin\Release\net6.0-windows\FPSBoostPro.exe"
$iconPath = "E:\fpsboost\app.ico"
$shortcutPath = Join-Path $desktopPath "FPSBoost Pro.lnk"

$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = "E:\fpsboost"
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

Write-Output "Atalho criado com sucesso: $shortcutPath"
