# Create a class library project for a .NET solution
# Must be run from the directory containing the solution file.
param(
    # Name of the new class library project, e.g. "MyApp.Core"
    [Parameter(Mandatory=$true)]
    [string]$ProjectName,

    # When specified, adds the GitHub.Copilot.SDK NuGet package and copies the CopilotService/ICopilotClient templates into the project
    [switch]$WithCopilot
)

if (-not (Get-ChildItem -Path $PWD -Filter "*.sln*" -File | Select-Object -First 1)) {
    Write-Error "No solution file found in the current directory. Run this script from the solution root."
    exit 1
}

Write-Host "=== Creating class library project... ==="
Write-Host ""

$projectPath = Join-Path $PWD $ProjectName

dotnet new classlib -n $ProjectName -o $projectPath
dotnet sln add $projectPath

# Delete the default Class1.cs file created by the template
$class1Path = Join-Path $projectPath "Class1.cs"
if (Test-Path $class1Path) {
    Remove-Item $class1Path
}

if ($WithCopilot) {
    Write-Host "=== Adding Copilot support... ==="

    dotnet add $projectPath package GitHub.Copilot.SDK

    $copilotTemplateRoot = Join-Path $PSScriptRoot "CopilotFiles"
    $copilotTemplateFiles = Get-ChildItem -Path $copilotTemplateRoot -Filter "*.template" -File
    foreach ($templateFile in $copilotTemplateFiles) {
        $targetFileName = [System.IO.Path]::GetFileNameWithoutExtension($templateFile.Name)
        $targetFilePath = Join-Path $projectPath $targetFileName

        $content = Get-Content -Path $templateFile.FullName -Raw
        $content = $content -replace "%NAMESPACE%", $ProjectName

        Set-Content -Path $targetFilePath -Value $content -NoNewline
    }
}

# Rebuild
dotnet build --no-incremental

Write-Host "=== Class Library Project Created Successfully ==="
Write-Host "Location: $projectPath"
