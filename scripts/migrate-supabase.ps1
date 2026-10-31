$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($env:SUPABASE_CS)) {
    throw "Thiếu SUPABASE_CS. Hãy đặt chuỗi kết nối Session pooler trong biến môi trường."
}

$parts = @{}
foreach ($part in $env:SUPABASE_CS.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
    $pair = $part.Split('=', 2)
    if ($pair.Count -eq 2) { $parts[$pair[0].Trim().ToLowerInvariant()] = $pair[1].Trim() }
}
$targetHost = $parts['host']
if ([string]::IsNullOrWhiteSpace($targetHost)) { throw "SUPABASE_CS không có Host hợp lệ." }

Write-Host "Đích migration: $targetHost (mật khẩu đã được che)"
$confirmation = Read-Host "Nhập MIGRATE $targetHost để tiếp tục"
if ($confirmation -cne "MIGRATE $targetHost") {
    Write-Host "Đã hủy; chưa có thay đổi nào được áp dụng."
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$env:ConnectionStrings__Default = $env:SUPABASE_CS
Push-Location $repoRoot
try {
    dotnet ef database update --project src/Restaurant.Infrastructure --startup-project src/Restaurant.Api
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
    Remove-Item Env:ConnectionStrings__Default -ErrorAction SilentlyContinue
}
