$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$outputDirectory = Join-Path $root 'docs/openapi'
$outputPath = Join-Path $outputDirectory 'openapi.v1.json'
$url = 'http://127.0.0.1:5099'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$process = Start-Process -FilePath 'dotnet' `
    -ArgumentList @('run', '--no-build', '--project', (Join-Path $root 'src/Restaurant.Api'), '--urls', $url) `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru

try {
    $response = $null
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri "$url/swagger/v1/swagger.json" -UseBasicParsing
            break
        }
        catch {
            Start-Sleep -Milliseconds 250
        }
    }
    if ($null -eq $response) { throw 'API không khởi động kịp để export OpenAPI.' }
    [System.IO.File]::WriteAllText($outputPath, $response.Content, [System.Text.UTF8Encoding]::new($false))
    Write-Host "Đã ghi $outputPath"
}
finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
}
