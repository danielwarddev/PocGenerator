#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Finds and displays all unique events from Copilot SDK logs.

.DESCRIPTION
    Parses log files and extracts unique event names from entries in the format:
    [timestamp] [level] [COPILOT][event_name] session=guid

.PARAMETER LogPath
    Path to the log file or directory containing log files.
    Defaults to current directory if not specified.

.PARAMETER Pattern
    Optional regex pattern to filter events. If specified, only events matching this pattern are included.

.PARAMETER OutputCsv
    Optional path to export results as a CSV file.

.PARAMETER SortByCount
    If specified, sorts results by event count (descending) instead of alphabetically.

.EXAMPLE
    .\Find-UniqueLogEvents.ps1 -LogPath "C:\logs\app.log"
    
.EXAMPLE
    .\Find-UniqueLogEvents.ps1 -LogPath "C:\logs" -SortByCount
    
.EXAMPLE
    .\Find-UniqueLogEvents.ps1 -LogPath "C:\logs" -OutputCsv "events.csv"
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$LogPath = ".",
    
    [Parameter(Mandatory = $false)]
    [string]$Pattern = $null,
    
    [Parameter(Mandatory = $false)]
    [string]$OutputCsv = $null,
    
    [Parameter(Mandatory = $false)]
    [switch]$SortByCount
)

# Resolve the path
$resolvedPath = Resolve-Path -Path $LogPath -ErrorAction Stop

# Get log files
$logFiles = if ((Get-Item $resolvedPath) -is [System.IO.DirectoryInfo]) {
    Get-ChildItem -Path $resolvedPath -Filter "*.log" -File -Recurse
} else {
    Get-Item -Path $resolvedPath
}

if (-not $logFiles) {
    Write-Error "No log files found at '$LogPath'"
    exit 1
}

# Extract unique events
$events = @()

foreach ($file in $logFiles) {
    Write-Verbose "Scanning $($file.FullName)..."
    
    $content = Get-Content -Path $file.FullName -ErrorAction SilentlyContinue
    if ($null -eq $content) {
        continue
    }
    
    # Convert to array if single line
    if ($content -is [string]) {
        $content = @($content)
    }
    
    # Match pattern: [COPILOT][event_name]
    foreach ($line in $content) {
        if ($line -match '\[COPILOT\]\[([^\]]+)\]') {
            $eventName = $Matches[1]
            
            # Apply pattern filter if specified
            if ($Pattern -and $eventName -notmatch $Pattern) {
                continue
            }
            
            $events += $eventName
        }
    }
}

if ($events.Count -eq 0) {
    Write-Warning "No Copilot events found in logs"
    exit 0
}

# Build event count objects
$eventCounts = @()
$uniqueEvents = $events | Select-Object -Unique

foreach ($uniqueEvent in $uniqueEvents) {
    $count = ($events -eq $uniqueEvent).Count
    $eventCounts += [PSCustomObject]@{
        Event = $uniqueEvent
        Count = $count
    }
}

# Sort results
if ($SortByCount) {
    $eventCounts = $eventCounts | Sort-Object -Property Count -Descending
} else {
    $eventCounts = $eventCounts | Sort-Object -Property Event
}

# Export to CSV if requested
if ($OutputCsv) {
    $eventCounts | Export-Csv -Path $OutputCsv -NoTypeInformation
    Write-Host "Results exported to: $OutputCsv" -ForegroundColor Green
}

# Display results
Write-Host "Found $($eventCounts.Count) unique event(s):`n" -ForegroundColor Green
Write-Host "Event Name                      Count" -ForegroundColor Cyan
Write-Host "---------------------------------------" -ForegroundColor Cyan

foreach ($item in $eventCounts) {
    $pad = $item.Event.PadRight(32)
    Write-Host "$pad $($item.Count)" 
}

# Summary
$totalEvents = $events.Count
$avgCount = [Math]::Round($totalEvents / $eventCounts.Count, 2)
Write-Host "`nSummary:" -ForegroundColor Green
Write-Host "  Total events logged: $totalEvents"
Write-Host "  Unique event types: $($eventCounts.Count)"
Write-Host "  Average per type: $avgCount"
