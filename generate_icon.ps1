Add-Type -AssemblyName System.Drawing

function Generate-GamerIcon {
    param([string]$outputPath)
    
    $sizes = @(256, 64, 48, 32, 16)
    $pngStreams = @()
    
    foreach ($sz in $sizes) {
        $bmp = New-Object System.Drawing.Bitmap($sz, $sz, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.Clear([System.Drawing.Color]::Transparent)
        
        $scale = [float]$sz / 256.0
        
        # 1. Dark Badge / Hexagonal Rounded Shield Background
        $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
            (New-Object System.Drawing.PointF(0, 0)),
            (New-Object System.Drawing.PointF($sz, $sz)),
            [System.Drawing.Color]::FromArgb(255, 26, 8, 12),
            [System.Drawing.Color]::FromArgb(255, 8, 4, 6)
        )
        
        $margin = 12.0 * $scale
        $w = $sz - (2.0 * $margin)
        $h = $sz - (2.0 * $margin)
        $r = 42.0 * $scale
        
        $path = New-Object System.Drawing.Drawing2D.GraphicsPath
        $rect = New-Object System.Drawing.RectangleF($margin, $margin, $w, $h)
        $path.AddArc($rect.X, $rect.Y, $r*2, $r*2, 180, 90)
        $path.AddArc($rect.Right - $r*2, $rect.Y, $r*2, $r*2, 270, 90)
        $path.AddArc($rect.Right - $r*2, $rect.Bottom - $r*2, $r*2, $r*2, 0, 90)
        $path.AddArc($rect.X, $rect.Bottom - $r*2, $r*2, $r*2, 90, 90)
        $path.CloseFigure()
        
        # Fill Background
        $g.FillPath($bgBrush, $path)
        
        # Outer Crimson Glow Pen
        $glowPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(120, 255, 30, 66), [float](10.0 * $scale))
        $g.DrawPath($glowPen, $path)
        
        # Inner Crisp Crimson Border
        $borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 30, 66), [float](4.0 * $scale))
        $g.DrawPath($borderPen, $path)
        
        # 2. Gaming Lightning Bolt Emblem
        $points = @(
            (New-Object System.Drawing.PointF([float](144.0 * $scale), [float](32.0 * $scale))),
            (New-Object System.Drawing.PointF([float](72.0 * $scale),  [float](140.0 * $scale))),
            (New-Object System.Drawing.PointF([float](122.0 * $scale), [float](140.0 * $scale))),
            (New-Object System.Drawing.PointF([float](96.0 * $scale),  [float](224.0 * $scale))),
            (New-Object System.Drawing.PointF([float](188.0 * $scale), [float](112.0 * $scale))),
            (New-Object System.Drawing.PointF([float](138.0 * $scale), [float](112.0 * $scale)))
        )
        
        $boltPath = New-Object System.Drawing.Drawing2D.GraphicsPath
        $boltPath.AddLines($points)
        $boltPath.CloseFigure()
        
        # Neon Red Glow behind the Bolt
        $boltGlowPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(160, 255, 30, 66), [float](12.0 * $scale))
        $g.DrawPath($boltGlowPen, $boltPath)
        
        # Crimson gradient fill for the bolt
        $boltBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
            (New-Object System.Drawing.PointF(0, [float](30.0 * $scale))),
            (New-Object System.Drawing.PointF(0, [float](225.0 * $scale))),
            [System.Drawing.Color]::FromArgb(255, 255, 60, 80),
            [System.Drawing.Color]::FromArgb(255, 220, 20, 45)
        )
        $g.FillPath($boltBrush, $boltPath)
        
        # White hot core highlight
        $corePoints = @(
            (New-Object System.Drawing.PointF([float](140.0 * $scale), [float](50.0 * $scale))),
            (New-Object System.Drawing.PointF([float](94.0 * $scale),  [float](134.0 * $scale))),
            (New-Object System.Drawing.PointF([float](126.0 * $scale), [float](134.0 * $scale))),
            (New-Object System.Drawing.PointF([float](110.0 * $scale), [float](205.0 * $scale))),
            (New-Object System.Drawing.PointF([float](172.0 * $scale), [float](118.0 * $scale))),
            (New-Object System.Drawing.PointF([float](136.0 * $scale), [float](118.0 * $scale)))
        )
        $corePath = New-Object System.Drawing.Drawing2D.GraphicsPath
        $corePath.AddLines($corePoints)
        $corePath.CloseFigure()
        $coreBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(235, 255, 255, 255))
        $g.FillPath($coreBrush, $corePath)
        
        # Save frame to PNG memory stream
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngStreams += $ms
        
        $g.Dispose()
        $bmp.Dispose()
    }
    
    # Write standard Windows ICO with PNG payloads
    $fs = [System.IO.File]::Create($outputPath)
    $bw = New-Object System.IO.BinaryWriter($fs)
    
    # Header: Reserved (0), Type (1), Count
    $bw.Write([uint16]0)
    $bw.Write([uint16]1)
    $bw.Write([uint16]$sizes.Count)
    
    $offset = 6 + (16 * $sizes.Count)
    
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $sz = $sizes[$i]
        $w = if ($sz -ge 256) { 0 } else { $sz }
        $h = if ($sz -ge 256) { 0 } else { $sz }
        $len = $pngStreams[$i].Length
        
        $bw.Write([byte]$w)
        $bw.Write([byte]$h)
        $bw.Write([byte]0)   # ColorCount
        $bw.Write([byte]0)   # Reserved
        $bw.Write([uint16]1)  # Planes
        $bw.Write([uint16]32) # BitCount
        $bw.Write([uint32]$len) # BytesInRes
        $bw.Write([uint32]$offset) # ImageOffset
        
        $offset += $len
    }
    
    for ($i = 0; $i -lt $sizes.Count; $i++) {
        $bytes = $pngStreams[$i].ToArray()
        $bw.Write($bytes)
        $pngStreams[$i].Dispose()
    }
    
    $bw.Flush()
    $fs.Close()
}

Generate-GamerIcon "E:\fpsboost\app.ico"
Write-Output "SUCCESS: E:\fpsboost\app.ico generated."
