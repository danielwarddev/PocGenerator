<#
.SYNOPSIS
    Analyzes PocGenerator Copilot log files and reports all "view" tool usages.

.DESCRIPTION
    Finds every view tool invocation across one or more log files, normalizes the
    paths to be relative to the MVP output directory (when possible), and counts
    how many times each unique path was viewed — both per-file and in aggregate.

.PARAMETER RepoRoot
    Absolute path to the repository root (the folder containing PocGenerator.slnx).
    Defaults to the parent of the directory this script lives in.

.PARAMETER LogPath
    Glob pattern or directory to search for log files.
    Defaults to the standard log output directory under RepoRoot.

.EXAMPLE
    .\Find-ViewToolUsage.ps1
    .\Find-ViewToolUsage.ps1 -RepoRoot "C:\work\PocGenerator"
    .\Find-ViewToolUsage.ps1 -LogPath "C:\work\PocGenerator\PocGenerator\bin\Debug\net10.0\logs\*.log"
#>
param(
    [string] $RepoRoot = (Resolve-Path "$PSScriptRoot\..").Path,

    [string] $LogPath,

    # When set, writes a CSV of the aggregate counts to this path instead of printing per-file tables
    [string] $CsvOutput
)

if (-not $LogPath) {
    $LogPath = Join-Path $RepoRoot 'PocGenerator\bin\Debug\net10.0\logs\pocgenerator-copilot-*.log'
}

$logFiles = Get-Item $LogPath -ErrorAction Stop | Sort-Object Name

if (-not $logFiles) {
    Write-Error "No log files found matching: $LogPath"
    exit 1
}

# Regex to extract the "path" argument from a view tool.execution_start line
$viewPattern = '\[tool\.execution_start\].*?"toolName":"view","arguments":\{"path":"((?:[^"\\]|\\.)*)"\}'

# Aggregates across all files: path -> total count
$globalCounts = @{}

foreach ($file in $logFiles) {
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor Cyan
    Write-Host "  $($file.Name)" -ForegroundColor Cyan
    Write-Host ("=" * 70) -ForegroundColor Cyan

    $fileCounts = @{}
    $total = 0

    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        if ($line -notmatch '\[tool\.execution_start\]') { continue }
        if ($line -notmatch '"toolName":"view"') { continue }

        if ($line -match $viewPattern) {
            # Unescape JSON backslashes
            $raw = $Matches[1] -replace '\\\\', '\'

            # Normalize: strip the long mvp-outputs prefix down to a relative path
            $buildOut = Join-Path $RepoRoot 'PocGenerator\bin\Debug\net10.0\'
            $srcDir   = Join-Path $RepoRoot 'PocGenerator\'
            $repoDir  = $RepoRoot.TrimEnd('\') + '\'
            $display  = $raw -replace [regex]::Escape($buildOut), '[build]\'
            $display  = $display -replace [regex]::Escape($srcDir),   '[src]\'
            $display  = $display -replace [regex]::Escape($repoDir),  '[repo]\'

            $fileCounts[$display] = ($fileCounts[$display] ?? 0) + 1
            $globalCounts[$display] = ($globalCounts[$display] ?? 0) + 1
            $total++
        }
    }

    if ($fileCounts.Count -eq 0) {
        Write-Host "  (no view calls found)" -ForegroundColor DarkGray
        continue
    }

    # Sort by count descending, then path ascending
    $sorted = $fileCounts.GetEnumerator() |
        Sort-Object @{ Expression = { $_.Value }; Descending = $true }, @{ Expression = { $_.Key } }

    $maxCount = ($sorted | Select-Object -First 1).Value
    $countWidth = [math]::Max(5, $maxCount.ToString().Length)

    foreach ($entry in $sorted) {
        $countStr = $entry.Value.ToString().PadLeft($countWidth)
        $bar = "#" * [math]::Min($entry.Value, 40)
        if ($entry.Value -ge 5) {
            $color = "Red"
        } elseif ($entry.Value -ge 3) {
            $color = "Yellow"
        } else {
            $color = "Green"
        }
        Write-Host ("  {0}x  {1,-60}  {2}" -f $countStr, $entry.Key, $bar) -ForegroundColor $color
    }

    Write-Host ""
    Write-Host ("  Total view calls: {0}  |  Unique paths: {1}" -f $total, $fileCounts.Count) -ForegroundColor DarkCyan
}

# --- Global Summary ---
Write-Host ""
Write-Host ("=" * 70) -ForegroundColor Magenta
Write-Host "  AGGREGATE ACROSS ALL LOGS" -ForegroundColor Magenta
Write-Host ("=" * 70) -ForegroundColor Magenta

$sortedGlobal = $globalCounts.GetEnumerator() |
    Sort-Object @{ Expression = { $_.Value }; Descending = $true }, @{ Expression = { $_.Key } }

$maxGlobal = ($sortedGlobal | Select-Object -First 1).Value
$globalCountWidth = [math]::Max(5, $maxGlobal.ToString().Length)

foreach ($entry in $sortedGlobal) {
    $countStr = $entry.Value.ToString().PadLeft($globalCountWidth)
    $bar = "#" * [math]::Min($entry.Value, 40)
    if ($entry.Value -ge 10) {
        $color = "Red"
    } elseif ($entry.Value -ge 5) {
        $color = "Yellow"
    } else {
        $color = "Green"
    }
    Write-Host ("  {0}x  {1,-60}  {2}" -f $countStr, $entry.Key, $bar) -ForegroundColor $color
}

$grandTotal = ($globalCounts.Values | Measure-Object -Sum).Sum
Write-Host ""
Write-Host ("  Grand total view calls: {0}  |  Unique paths: {1}" -f $grandTotal, $globalCounts.Count) -ForegroundColor DarkMagenta

if ($CsvOutput) {
    $sortedGlobal |
        Select-Object @{N='Count';E={$_.Value}}, @{N='Path';E={$_.Key}} |
        Export-Csv -Path $CsvOutput -NoTypeInformation -Encoding UTF8
    Write-Host ""
    Write-Host "  CSV written to: $CsvOutput" -ForegroundColor Cyan
}
