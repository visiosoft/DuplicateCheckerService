Add-Type -AssemblyName System.Drawing

function Create-PlaceholderImage {
    param (
        [string]$Path,
        [int]$Width,
        [int]$Height,
        [string]$Text
    )
    
    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    
    # Fill background
    $graphics.Clear([System.Drawing.Color]::White)
    
    # Draw border
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::Blue), 2
    $graphics.DrawRectangle($pen, 0, 0, $Width - 1, $Height - 1)
    
    # Draw text
    $font = New-Object System.Drawing.Font "Arial", ($Width / 10)
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::Black)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    
    $graphics.DrawString($Text, $font, $brush, ($Width / 2), ($Height / 2), $format)
    
    # Save image
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Clean up
    $graphics.Dispose()
    $bitmap.Dispose()
}

# Create images directory if it doesn't exist
$assetsDir = "DuplicateCheckerService\Assets"
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force
}

# Create placeholder images
Create-PlaceholderImage -Path "$assetsDir\StoreLogo.png" -Width 50 -Height 50 -Text "DC"
Create-PlaceholderImage -Path "$assetsDir\Square150x150Logo.png" -Width 150 -Height 150 -Text "DC"
Create-PlaceholderImage -Path "$assetsDir\Square44x44Logo.png" -Width 44 -Height 44 -Text "DC" 