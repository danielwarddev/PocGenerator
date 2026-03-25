# Create a test project for an existing .NET project
# Must be run from the directory containing the solution file.
param(
    # Name of the source project to create tests for, e.g. "MyApp.Api"
    [Parameter(Mandatory=$true)]
    [string]$SourceProjectName
)

if (-not (Get-ChildItem -Path $PWD -Filter "*.sln*" -File | Select-Object -First 1)) {
    Write-Error "No solution file found in the current directory. Run this script from the solution root."
    exit 1
}

Write-Host "=== Creating test project... ==="
Write-Host ""

$testProjectName = "$SourceProjectName.UnitTests"
$testProjectPath = Join-Path $PWD $testProjectName
$sourceProjectPath = Join-Path $PWD $SourceProjectName

# Check if test project already exists
if (Test-Path $testProjectPath) {
    Write-Host "Test project already exists at: $testProjectPath"
    exit 0
}

# Create the test project using xUnit template
dotnet new xunit -n $testProjectName -o $testProjectPath

# Add to solution
dotnet sln add $testProjectPath

# Add NuGet packages
dotnet add $testProjectPath package AwesomeAssertions
dotnet add $testProjectPath package AutoFixture
dotnet add $testProjectPath package NSubstitute

# Add project reference
dotnet add $testProjectPath reference $sourceProjectPath

# Delete the default UnitTest1.cs file created by the template
$unitTest1Path = Join-Path $testProjectPath "UnitTest1.cs"
if (Test-Path $unitTest1Path) {
    Remove-Item $unitTest1Path
}

# Rebuild
dotnet build --no-incremental

Write-Host "=== Test Project Created Successfully ==="
Write-Host "Location: $testProjectPath"