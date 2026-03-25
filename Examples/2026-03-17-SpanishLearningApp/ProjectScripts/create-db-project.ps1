# Create a database class library project for a .NET solution
# Must be run from the directory containing the solution file.
param(
    # Name of the new database project, e.g. "MyApp.Database"
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

if (-not (Get-ChildItem -Path $PWD -Filter "*.sln*" -File | Select-Object -First 1)) {
    Write-Error "No solution file found in the current directory. Run this script from the solution root."
    exit 1
}

Write-Host "=== Creating database project... ==="
Write-Host ""

$projectPath = Join-Path $PWD $ProjectName

dotnet new classlib -n $ProjectName -o $projectPath
dotnet sln add $projectPath

dotnet add $projectPath package Microsoft.EntityFrameworkCore.Design
dotnet add $projectPath package Npgsql.EntityFrameworkCore.PostgreSQL

# Delete the default Class1.cs file created by the template
$class1Path = Join-Path $projectPath "Class1.cs"
if (Test-Path $class1Path) {
    Remove-Item $class1Path
}

# Rebuild
dotnet build --no-incremental

Write-Host "=== Database Project Created Successfully ==="
Write-Host "Location: $projectPath"
