param(
    [switch]$NoPause
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-LastExitCodeOrDefault {
    param(
        [int]$Default = 1
    )

    $lastExitCode = Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue
    if ($null -ne $lastExitCode) {
        return [int]$lastExitCode.Value
    }

    return $Default
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$exitCode = 0
$statusMessage = "Verification complete."

try {
    Push-Location $repoRoot
    try {
        & dotnet test ".\Core.Tests\Core.Tests.csproj"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "dotnet test failed with exit code $exitCode."
        }

        & dotnet build ".\Core\Core.csproj" --configuration Release
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "dotnet build failed with exit code $exitCode."
        }

        $exitCode = 0
    }
    finally {
        Pop-Location
    }
}
catch {
    if ($exitCode -eq 0) {
        $exitCode = Get-LastExitCodeOrDefault
    }

    $statusMessage = "Verification failed: $($_.Exception.Message)"
    Write-Error $statusMessage
}

if (-not $NoPause) {
    Write-Host
    Read-Host "$statusMessage Press Enter to close"
}

exit $exitCode
