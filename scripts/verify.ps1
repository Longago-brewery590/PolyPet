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

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$exitCode = 0
$statusMessage = "Verification complete."

try {
    Push-Location $repoRoot
    try {
        & bash "./scripts/test-sync-scripts.sh"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "sync script smoke test failed with exit code $exitCode."
        }

        & bash "./scripts/sync-core-to-adapters.sh"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "sync Core to adapters failed with exit code $exitCode."
        }

        & bash "./scripts/sync-godot-sample.sh"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "sync Godot sample failed with exit code $exitCode."
        }

        & bash "./scripts/sync-unity-sample.sh"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "sync Unity sample failed with exit code $exitCode."
        }

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

        & dotnet build ".\Samples\PolyPetDemoGodot\PolyPetDemoGodot.csproj"
        $exitCode = Get-LastExitCodeOrDefault -Default 0
        if ($exitCode -ne 0) {
            throw "Godot sample build failed with exit code $exitCode."
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
        if ($exitCode -eq 0) {
            $exitCode = 1
        }
    }

    $statusMessage = "Verification failed: $($_.Exception.Message)"
    Write-Error $statusMessage
}

if (-not $NoPause) {
    Write-Host
    Read-Host "$statusMessage Press Enter to close"
}

exit $exitCode
