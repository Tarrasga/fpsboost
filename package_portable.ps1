$publishDir = "E:\fpsboost\bin\Release\net6.0-windows\win-x64\publish"

# Garantir que o app.ico está na pasta de publicação
Copy-Item "E:\fpsboost\app.ico" -Destination $publishDir -Force

# Criar LEIA-ME.txt
$readmeContent = @"
========================================================================
             FPSBOOST PRO - GUIA RÁPIDO PARA O TEU PC
========================================================================

Este programa foi desenhado para eliminar micro-travamentos (stuttering),
reduzir a latência de rede (ping e jitter) e maximizar o FPS em jogos
como Rust, Counter-Strike 2 (CS2), PUBG e outros jogos competitivos.

COMO USAR (EM 3 PASSOS SIMPLES):
------------------------------------------------------------------------
1. Extrai todo o conteúdo deste ficheiro ZIP para uma pasta à tua escolha
   (por exemplo: C:\FPSBoostPro ou no teu Ambiente de Trabalho).

2. (Opcional) Clica duas vezes em 'Criar_Atalho_Ambiente_Trabalho.bat'
   para colocar o atalho com o ícone gamer no teu Ambiente de Trabalho.

3. Executa o 'Iniciar_FPSBoost.bat' (ou 'FPSBoostPro.exe') como
   Administrador.
   Dentro da aplicação, clica no botão grande:
   '🚀 OTIMIZAR AGORA (1-CLIQUE)' antes de jogares!

O QUE A APLICAÇÃO FAZ PELO TEU COMPUTADOR:
------------------------------------------------------------------------
- Ativa o Plano de Elevado Desempenho (acaba com o adormecimento de núcleos da CPU).
- Desativa o Algoritmo de Nagle na placa de rede (envio instantâneo de pacotes de tiros).
- Otimiza a prioridade do Windows (SystemResponsiveness = 0) para entregar 100% da CPU ao jogo.
- Ativa o Agendamento de GPU por Hardware (HAGS) para menor atraso de clique.
- Deteta quando abres Rust, CS2 ou PUBG e aplica temporizador de alta precisão (1ms) e limpeza de RAM.
- Totalmente seguro e reversível (inclui aba de Segurança & Backup com 1-clique).
"@

$readmePath = Join-Path $publishDir "LEIA-ME.txt"
[System.IO.File]::WriteAllText($readmePath, $readmeContent, [System.Text.Encoding]::UTF8)

# Criar ZIP portátil pronto a enviar
$zipPath = "E:\fpsboost\FPSBoostPro_Portatil.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Output "A compactar pacote portátil para ZIP..."
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal

$zipItem = Get-Item $zipPath
$sizeMb = [math]::Round($zipItem.Length / 1MB, 2)
Write-Output "SUCESSO: Pacote portátil criado em: $zipPath ($sizeMb MB)"
