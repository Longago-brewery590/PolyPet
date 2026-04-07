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
$sourceDir = Join-Path $repoRoot "Samples\PolyPetDemoUnity\Packages\com.shilo.polypet"
$destDir = Join-Path $repoRoot "Unity"
$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("polypet-unity-sync-" + [System.Guid]::NewGuid().ToString("N"))
$exitCode = 0
$statusMessage = "Unity package copy complete."

try {
    if (-not (Test-Path -LiteralPath $sourceDir -PathType Container)) {
        throw "Sample Unity package folder not found: $sourceDir"
    }

    $packageJson = Join-Path $sourceDir "package.json"
    if (-not (Test-Path -LiteralPath $packageJson -PathType Leaf)) {
        throw "Sample Unity package is missing package.json: $packageJson"
    }

    $destParent = Split-Path -Parent $destDir
    New-Item -ItemType Directory -Path $stagingRoot | Out-Null

    $stagedPackage = Join-Path $stagingRoot "Unity"
    Copy-Item -LiteralPath $sourceDir -Destination $stagedPackage -Recurse -Force

    New-Item -ItemType Directory -Path $destParent -Force | Out-Null
    if (Test-Path -LiteralPath $destDir) {
        Remove-Item -LiteralPath $destDir -Recurse -Force
    }

    Move-Item -LiteralPath $stagedPackage -Destination $destDir
    Write-Host "Copied sample Unity package into $destDir"

    # Sync sample scene/assets back to the package's Samples~ folder.
    # The scene is authored in the sample project's Assets/PolyPetCreator/ and
    # must be kept in sync with Unity/Samples~/PolyPetCreator/ (the source of truth
    # that sync-unity-sample.sh distributes).
    $sampleAssetsDir = Join-Path $repoRoot "Samples\PolyPetDemoUnity\Assets\PolyPetCreator"
    $samplesDest = Join-Path $destDir "Samples~\PolyPetCreator"
    if (Test-Path -LiteralPath $sampleAssetsDir -PathType Container) {
        if (Test-Path -LiteralPath $samplesDest) {
            Remove-Item -LiteralPath $samplesDest -Recurse -Force
        }
        New-Item -ItemType Directory -Path $samplesDest -Force | Out-Null
        Copy-Item -Path (Join-Path $sampleAssetsDir "*") -Destination $samplesDest -Recurse -Force
        Write-Host "Copied sample assets into $samplesDest"
    }
}
catch {
    if ($exitCode -eq 0) {
        $exitCode = Get-LastExitCodeOrDefault
        if ($exitCode -eq 0) {
            $exitCode = 1
        }
    }

    $statusMessage = "Unity package copy failed: $($_.Exception.Message)"
    Write-Error $statusMessage
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }

    if (-not $NoPause) {
        Write-Host
        Read-Host "$statusMessage Press Enter to close"
    }
}

exit $exitCode
