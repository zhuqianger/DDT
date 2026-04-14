param()

# Step 1: Read hook input JSON from stdin.
$raw = [Console]::In.ReadToEnd()

# Step 2: Allow when input is empty.
if ([string]::IsNullOrWhiteSpace($raw)) {
  '{ "permission": "allow" }'
  exit 0
}

# Step 3: Parse JSON; fail-open for this demo.
try {
  $inputJson = $raw | ConvertFrom-Json
} catch {
  '{ "permission": "allow" }'
  exit 0
}

# Step 4: Extract command string.
$command = ""
if ($null -ne $inputJson.command) {
  $command = [string]$inputJson.command
}

# Step 5 (matching rule): This regex is the key matcher.
# - (?i): case-insensitive
# - Only match one risky command in this simplified example.
$dangerousPattern = '(?i)git\s+reset\s+--hard'

# Step 6: If matched, require confirmation.
if ($command -match $dangerousPattern) {
  $result = @{
    permission   = "ask"
    user_message = "Potentially dangerous command detected: $command`nPlease confirm before continuing."
    agent_message = "Hook matched the risky command pattern and changed permission to ask."
  }
  $result | ConvertTo-Json -Compress
  exit 0
}

# Step 7: Allow when not matched.
'{ "permission": "allow" }'
exit 0
