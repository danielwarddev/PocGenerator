# Create a Blazor project for a .NET solution
# Must be run from the directory containing the solution file.
param(
    # Name of the new Blazor project, e.g. "MyApp.Web"
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

if (-not (Get-ChildItem -Path $PWD -Filter "*.sln*" -File | Select-Object -First 1)) {
    Write-Error "No solution file found in the current directory. Run this script from the solution root."
    exit 1
}

Write-Host "=== Creating Blazor project... ==="
Write-Host ""

$projectPath = Join-Path $PWD $ProjectName

dotnet new blazor -n $ProjectName -o $projectPath

dotnet sln add $projectPath

dotnet add $projectPath package Radzen.Blazor

# --- Inject Radzen into generated files at known anchors ---

$appRazor = Join-Path $projectPath "Components\App.razor"
$content = Get-Content -Path $appRazor -Raw

# App.razor: Insert <RadzenTheme> at the top of <head>
$content = $content -replace "(<head>)", "`$1`n    <RadzenTheme Theme=""material"" />"

# App.razor: Insert Radzen <script> right before </body>
$content = $content -replace "(</body>)", "    <script src=""_content/Radzen.Blazor/Radzen.Blazor.js""></script>`n`$1"
Set-Content -Path $appRazor -Value $content -NoNewline

# _Imports.razor: add Radzen usings at the end
$importsRazor = Join-Path $projectPath "Components\_Imports.razor"
$content = Get-Content -Path $importsRazor -Raw
$content = $content.TrimEnd() + "`n@using Radzen`n@using Radzen.Blazor`n"
Set-Content -Path $importsRazor -Value $content -NoNewline

# Program.cs: add using Radzen; to top and  .AddRadzenComponents() right before AddRazorComponents()
$programCs = Join-Path $projectPath "Program.cs"
$content = Get-Content -Path $programCs -Raw
$content = $content -replace "(using $([regex]::Escape($ProjectName)))", "using Radzen;`n`$1"
$content = $content -replace "(\.AddRazorComponents\(\))", ".AddRadzenComponents()`n    `$1"
Set-Content -Path $programCs -Value $content -NoNewline

# Rebuild
dotnet build --no-incremental

Write-Host "=== Blazor Project Created Successfully ==="
Write-Host "Location: $projectPath"
