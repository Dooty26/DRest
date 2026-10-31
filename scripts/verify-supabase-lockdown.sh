#!/usr/bin/env bash
set -euo pipefail

: "${SUPABASE_URL:?Thiếu SUPABASE_URL}"
: "${SUPABASE_PUBLISHABLE_KEY:?Thiếu SUPABASE_PUBLISHABLE_KEY}"

for table in roles reservations; do
  body_file="$(mktemp)"
  status="$(curl --silent --show-error --output "$body_file" --write-out '%{http_code}' \
    --header "apikey: $SUPABASE_PUBLISHABLE_KEY" \
    --header "Authorization: Bearer $SUPABASE_PUBLISHABLE_KEY" \
    "$SUPABASE_URL/rest/v1/$table?select=*&limit=1")"
  body="$(tr -d '[:space:]' < "$body_file")"
  rm -f "$body_file"
  if [[ "$status" == "401" || "$status" == "403" || ( "$status" == "200" && "$body" == "[]" ) ]]; then
    echo "OK $table: Data API bị từ chối hoặc trả tập rỗng ($status)."
  else
    echo "FAILED $table: Data API trả HTTP $status với dữ liệu ngoài mong đợi." >&2
    exit 1
  fi
done
