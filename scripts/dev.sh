#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
docker compose --file "$ROOT/docker-compose.yml" up -d postgres
dotnet run --project "$ROOT/src/Restaurant.Api"
