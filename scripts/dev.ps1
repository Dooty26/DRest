$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
docker compose --file (Join-Path $root 'docker-compose.yml') up -d postgres
dotnet run --project (Join-Path $root 'src/Restaurant.Api')
