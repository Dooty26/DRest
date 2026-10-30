#!/usr/bin/env node
// Quét chuỗi HIỂN THỊ trong Web (tsx/jsx/html) và WPF (xaml) để phát hiện nội dung kỹ thuật/thông tin thừa.
// Dùng:  node scripts/check-ui-copy.mjs [--strict] [--json]
//   --strict : cảnh báo (câu quá dài) cũng tính là lỗi
// Bỏ qua một dòng có lý do chính đáng:  // copy-ok: <lý do>   hoặc   <!-- copy-ok: <lý do> -->
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

const ROOT = process.cwd();
const TARGETS = [
  { dir: 'web/src', exts: ['.tsx', '.jsx', '.html'] },
  { dir: 'web', exts: ['.html'], shallow: true },
  { dir: 'src/Restaurant.Wpf', exts: ['.xaml'] },
];
const SKIP_DIR = new Set(['node_modules', 'bin', 'obj', 'dist', '.git', 'mocks', '__tests__', 'tests', 'evidence']);
const SKIP_FILE = /(\.test\.|\.spec\.|\.d\.ts$|schema\.d\.ts$)/;
const STRICT = process.argv.includes('--strict');
const AS_JSON = process.argv.includes('--json');
const LONG_TEXT = 140; // ký tự — nhiều hơn thì cảnh báo "đoạn văn"

// [mẫu, mô tả]. Áp dụng CHỈ trên chuỗi hiển thị (xem extract*).
const BANNED = [
  [/antigravity|codex|claude|gemini/i, 'tên agent/công cụ'],
  [/\bpha\s*\d|\bphase\b|\bsprint\b/i, 'tên giai đoạn/prompt'],
  [/docs?\/\d{2}|§|theo đặc tả|theo hợp đồng|\bcontract\b/i, 'tham chiếu tài liệu'],
  [/\ba11y\b|\bwcag\b|touch\s*target|chạm\s*[≥>]|[≥>]=?\s*\d+\s*px|\b\d+\s*px\b/i, 'thuật ngữ truy cập/kích thước kỹ thuật'],
  [/\bdesign\s*(system|token)s?\b|\btokens?\b|\bui[\s-]?kit\b|\bstorybook\b/i, 'thuật ngữ design system'],
  [/\bmock\b|\bstub\b|\blorem\b|\bipsum\b|\btodo\b|\bfixme\b|\bplaceholder\b/i, 'nội dung giả/chưa hoàn thiện'],
  [/supabase|postgres|npgsql|signalr|openapi|swagger|\bmsw\b|\bapi\b|\bjson\b|\benum\b|\bendpoint\b|\bpayload\b|\bbackend\b|\bfrontend\b|viewmodel/i, 'thuật ngữ kỹ thuật'],
  [/\b(AVAILABLE|RESERVED|OCCUPIED|CLEANING|HELD|ESCALATED|PENDING_DEPOSIT|CONFIRMED|CHECKED_IN|NO_SHOW|SLOT_UNAVAILABLE|NOT_IMPLEMENTED)\b/, 'mã nội bộ chưa dịch'],
  [/lưới\s*\d+\s*phút|quy tắc\s*\d+\s*phút|\b105\s*phút|\bbuffer\b|\bnet\s*âm\b|cọc\s*>\s*bill|bắt buộc dùng chung/i, 'quy tắc nội bộ giải thích cho dev'],
];

// Thuộc tính chứa chữ hiển thị
const JSX_ATTR = 'title|alt|placeholder|aria-label|aria-description|aria-placeholder|label|helperText|description|subtitle|caption|heading|message|text|tooltip|content';
const XAML_ATTR = 'Text|Content|Header|ToolTip|Title|Watermark|Label|Description|Caption|PlaceholderText|AutomationProperties\\.Name|AutomationProperties\\.HelpText';

function* walk(dir, exts, shallow) {
  let entries; try { entries = readdirSync(dir); } catch { return; }
  for (const name of entries) {
    const p = join(dir, name);
    let st; try { st = statSync(p); } catch { continue; }
    if (st.isDirectory()) { if (!shallow && !SKIP_DIR.has(name)) yield* walk(p, exts, false); continue; }
    if (exts.some((e) => name.endsWith(e)) && !SKIP_FILE.test(name)) yield p;
  }
}

// Trả về [{text, col}] các chuỗi hiển thị trong 1 dòng
function extract(line, isXaml) {
  const out = [];
  const attr = isXaml ? XAML_ATTR : JSX_ATTR;
  const reAttr = new RegExp(`\\b(?:${attr})\\s*=\\s*(?:"([^"]*)"|'([^']*)'|\\{\\s*["'\`]([^"'\`]*)["'\`]\\s*\\})`, 'g');
  for (const m of line.matchAll(reAttr)) {
    const t = m[1] ?? m[2] ?? m[3];
    if (t && !/^\{?Binding|^\{StaticResource|^\{DynamicResource/.test(t)) out.push({ text: t, col: m.index });
  }
  // Text node giữa thẻ:  >Chữ<
  for (const m of line.matchAll(/>([^<>{}]{2,})</g)) {
    const t = m[1].trim();
    if (t && /[\p{L}]/u.test(t)) out.push({ text: t, col: m.index });
  }
  // JSX: {"chữ"} hoặc {'chữ'} đứng riêng làm nội dung
  if (!isXaml) for (const m of line.matchAll(/\{\s*["'`]([^"'`]{2,})["'`]\s*\}/g)) out.push({ text: m[1], col: m.index });
  return out;
}

const findings = [];
for (const t of TARGETS) {
  for (const file of walk(join(ROOT, t.dir), t.exts, t.shallow)) {
    const isXaml = file.endsWith('.xaml');
    const lines = readFileSync(file, 'utf8').split(/\r?\n/);
    lines.forEach((line, i) => {
      const prev = i > 0 ? lines[i - 1] : '';
      if (/copy-ok\s*:/.test(line) || /copy-ok\s*:/.test(prev)) return;
      if (/^\s*(\/\/|\/\*|\*|<!--)/.test(line)) return; // dòng comment thuần
      for (const { text } of extract(line, isXaml)) {
        for (const [re, why] of BANNED) {
          if (re.test(text)) findings.push({ level: 'error', file: relative(ROOT, file).split(sep).join('/'), line: i + 1, why, text });
        }
        if (text.length > LONG_TEXT) findings.push({ level: STRICT ? 'error' : 'warn', file: relative(ROOT, file).split(sep).join('/'), line: i + 1, why: `đoạn văn dài (${text.length} ký tự)`, text });
      }
    });
  }
}

const errors = findings.filter((f) => f.level === 'error');
if (AS_JSON) console.log(JSON.stringify(findings, null, 2));
else {
  for (const f of findings) console.log(`${f.level === 'error' ? 'LỖI ' : 'CẢNH BÁO'} ${f.file}:${f.line}  [${f.why}]  "${f.text.slice(0, 90)}${f.text.length > 90 ? '…' : ''}"`);
  console.log(`\n${errors.length} lỗi, ${findings.length - errors.length} cảnh báo.`);
}
process.exit(errors.length ? 1 : 0);
