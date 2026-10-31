$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($env:SUPABASE_URL)) { throw "Thiếu SUPABASE_URL." }
if ([string]::IsNullOrWhiteSpace($env:SUPABASE_PUBLISHABLE_KEY)) { throw "Thiếu SUPABASE_PUBLISHABLE_KEY." }

$headers = @{
    apikey = $env:SUPABASE_PUBLISHABLE_KEY
    Authorization = "Bearer $($env:SUPABASE_PUBLISHABLE_KEY)"
}

foreach ($table in @('roles', 'reservations')) {
    $uri = "$($env:SUPABASE_URL.TrimEnd('/'))/rest/v1/$table`?select=*&limit=1"
    try {
        $response = Invoke-WebRequest -Uri $uri -Headers $headers -Method Get -SkipHttpErrorCheck
        $status = [int]$response.StatusCode
        $body = $response.Content.Trim()
    }
    catch {
        throw "Không thể kiểm tra $table`: $($_.Exception.Message)"
    }
    if (($status -eq 200 -and $body -eq '[]') -or $status -in @(401, 403)) {
        Write-Host "OK $table`: Data API bị từ chối hoặc trả tập rỗng ($status)."
    }
    else {
        throw "FAILED $table`: Data API trả HTTP $status với dữ liệu ngoài mong đợi."
    }
}
