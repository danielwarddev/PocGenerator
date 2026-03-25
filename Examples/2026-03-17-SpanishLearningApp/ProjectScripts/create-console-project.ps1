# Create a console project for a .NET solution
# Must be run from the directory containing the solution file.
param(
    # Name of the new console project, e.g. "MyApp"
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

if (-not (Get-ChildItem -Path $PWD -Filter "*.sln*" -File | Select-Object -First 1)) {
    Write-Error "No solution file found in the current directory. Run this script from the solution root."
    exit 1
}

Write-Host "=== Creating console project... ==="
Write-Host ""

$projectPath = Join-Path $PWD $ProjectName

dotnet new console -n $ProjectName -o $projectPath

# Only for Program.cs.template, replace the entire contents of Program.cs with the template content (since Program.cs exists already and we want to replace the entire contents)
$templateRoot = Join-Path $PSScriptRoot "ConsoleFiles"
$programTemplatePath = Join-Path $templateRoot "Program.cs.template"
if (Test-Path $programTemplatePath) {
    $programContent = Get-Content -Path $programTemplatePath -Raw
    $programContent = $programContent -replace "%NAMESPACE%", $ProjectName
    Set-Content -Path (Join-Path $projectPath "Program.cs") -Value $programContent -NoNewline
}

# Apply all templates (except Program.cs.template) from ProjectScripts/ConsoleFiles into the generated project
$templateFiles = Get-ChildItem -Path $templateRoot -Filter "*.template" -File |
    Where-Object { $_.Name -ne "Program.cs.template" }
foreach ($templateFile in $templateFiles) {
    $targetFileName = [System.IO.Path]::GetFileNameWithoutExtension($templateFile.Name)
    $targetFilePath = Join-Path $projectPath $targetFileName

    $content = Get-Content -Path $templateFile.FullName -Raw
    $content = $content -replace "%NAMESPACE%", $ProjectName

    Set-Content -Path $targetFilePath -Value $content -NoNewline
}

dotnet sln add $projectPath

dotnet add $projectPath package Microsoft.Extensions.Hosting
dotnet add $projectPath package Serilog
dotnet add $projectPath package Serilog.Extensions.Hosting
dotnet add $projectPath package CommandLineParser

# Rebuild
dotnet build --no-incremental

Write-Host "=== Console Project Created Successfully ==="
Write-Host "Location: $projectPath"