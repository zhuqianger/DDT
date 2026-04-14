param()

# Read raw JSON from stdin.
$raw = [Console]::In.ReadToEnd()
$raw = $raw.TrimStart([char]0xFEFF)
# Some environments pass BOM/noise characters before JSON.
$jsonStart = $raw.IndexOf('{')
if ($jsonStart -ge 0) {
  $raw = $raw.Substring($jsonStart)
}

# Build log path under project hook directory.
$logPath = ".cursor/hooks/hook-debug.log"
$time = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Write-LogLine {
  param(
    [string]$Path,
    [string]$Line
  )
  [System.IO.File]::AppendAllText($Path, $Line + [Environment]::NewLine, $utf8NoBom)
}

$cmd = ""
$matchedBy = "none"
$rawForLog = $raw -replace '^[^\x20-\x7E]+', ''
$normalizedRaw = $rawForLog.Trim()
$prettyJson = ""
$obj = $null

# Try to reconstruct valid JSON shape for logging.
if ($normalizedRaw.StartsWith('"')) {
  $normalizedRaw = "{" + $normalizedRaw
}
$lastBrace = $normalizedRaw.LastIndexOf('}')
if ($lastBrace -ge 0) {
  $normalizedRaw = $normalizedRaw.Substring(0, $lastBrace + 1)
}

try {
  $obj = $normalizedRaw | ConvertFrom-Json
  $prettyJson = $obj | ConvertTo-Json -Depth 20
} catch {
  $prettyJson = "__PARSE_FAILED__"
}

# Prefer regex extraction because some environments inject non-JSON prefix chars.
$m = [regex]::Match($raw, '"command"\s*:\s*"(?<cmd>(?:\\.|[^"\\])*)"')
if ($m.Success) {
  $cmd = $m.Groups["cmd"].Value
  $cmd = $cmd -replace '\\\\', '\'
  $cmd = $cmd -replace '\\"', '"'
  $matchedBy = "regex"
} else {
  # Fallback: try JSON parse.
  try {
    $obj = $raw | ConvertFrom-Json
    if ($null -ne $obj.command) {
      $cmd = [string]$obj.command
      $matchedBy = "json"
    }
  } catch {
    $matchedBy = "failed"
  }
}

Write-LogLine -Path $logPath -Line "[$time] EVENT=beforeShellExecution"
Write-LogLine -Path $logPath -Line "[$time] RAW=$rawForLog"
Write-LogLine -Path $logPath -Line "[$time] RAW_JSON_NORMALIZED=$normalizedRaw"
Write-LogLine -Path $logPath -Line "[$time] RAW_JSON_PRETTY=$prettyJson"
Write-LogLine -Path $logPath -Line "[$time] COMMAND=$cmd"
Write-LogLine -Path $logPath -Line "[$time] MATCHED_BY=$matchedBy"
Write-LogLine -Path $logPath -Line "[$time] ---"

# Debug mode: always allow so you can inspect logs safely.
'{ "permission": "allow" }'
exit 0
