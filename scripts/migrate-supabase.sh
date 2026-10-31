#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${SUPABASE_CS:-}" ]]; then
  echo "Thiếu SUPABASE_CS. Hãy đặt chuỗi kết nối Session pooler trong biến môi trường." >&2
  exit 1
fi

target_host="$(printf '%s' "$SUPABASE_CS" | tr ';' '\n' | sed -n 's/^[Hh][Oo][Ss][Tt]=//p' | head -n 1)"
if [[ -z "$target_host" ]]; then
  echo "SUPABASE_CS không có Host hợp lệ." >&2
  exit 1
fi

echo "Đích migration: $target_host (mật khẩu đã được che)"
read -r -p "Nhập MIGRATE $target_host để tiếp tục: " confirmation
if [[ "$confirmation" != "MIGRATE $target_host" ]]; then
  echo "Đã hủy; chưa có thay đổi nào được áp dụng."
  exit 1
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"
ConnectionStrings__Default="$SUPABASE_CS" dotnet ef database update \
  --project src/Restaurant.Infrastructure \
  --startup-project src/Restaurant.Api
