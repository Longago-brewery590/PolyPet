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

        # Verify Unity sample assets are in sync with source of truth.
        $sampleSrc = "Unity/Samples~/PolyPetCreator"
        $sampleDst = "Samples/PolyPetDemoUnity/Assets/PolyPetCreator"
        if ((Test-Path $sampleSrc) -and (Test-Path $sampleDst)) {
            $srcFiles = Get-ChildItem -Path $sampleSrc -Recurse -File | ForEach-Object {
                $rel = $_.FullName.Substring((Resolve-Path $sampleSrc).Path.Length + 1)
                @{ Rel = $rel; Hash = (Get-FileHash $_.FullName -Algorithm SHA256).Hash }
            }
            foreach ($f in $srcFiles) {
                $dstPath = Join-Path $sampleDst $f.Rel
                if (-not (Test-Path $dstPath)) {
                    throw "Unity sample drift: $($f.Rel) exists in source of truth but not in sample project."
                }
                $dstHash = (Get-FileHash $dstPath -Algorithm SHA256).Hash
                if ($f.Hash -ne $dstHash) {
                    throw "Unity sample drift: $($f.Rel) differs between source of truth and sample project. Run sync-unity-sample.sh or sync-unity-package-from-sample.ps1."
                }
            }
            Write-Host "Unity sample assets are in sync."
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
