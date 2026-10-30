#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$ROOT/docs/openapi/openapi.v1.json"
URL="http://127.0.0.1:5099"
mkdir -p "$(dirname "$OUT")"
dotnet run --no-build --project "$ROOT/src/Restaurant.Api" --urls "$URL" >/tmp/restaurant-openapi.log 2>&1 &
PID=$!
trap 'kill "$PID" 2>/dev/null || true' EXIT
for _ in $(seq 1 60); do
  if curl --fail --silent "$URL/swagger/v1/swagger.json" --output "$OUT"; then
    exit 0
  fi
  sleep 0.25
done
cat /tmp/restaurant-openapi.log >&2
exit 1
