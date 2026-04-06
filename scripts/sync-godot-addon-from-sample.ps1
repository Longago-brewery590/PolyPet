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

function Strip-GodotUidFields {
    param(
        [string]$Root
    )

    $extensions = @(".cfg", ".import", ".res", ".theme", ".tscn", ".tres")
    $files = Get-ChildItem -LiteralPath $Root -Recurse -File | Where-Object { $_.Extension -in $extensions }

    foreach ($file in $files) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        $updated = $content `
            -replace '[ \t]+uid[ \t]*=[ \t]*"uid:\/\/[^"]*"', '' `
            -replace '(?m)^[ \t]*uid[ \t]*=[ \t]*"uid:\/\/[^"]*"\r?\n?', ''

        if ($updated -ne $content) {
            [System.IO.File]::WriteAllText($file.FullName, $updated)
        }
    }
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$sourceDir = Join-Path $repoRoot "Samples\PolyPetDemoGodot\addons\PolyPet"
$destDir = Join-Path $repoRoot "Godot\addons\PolyPet"
$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("polypet-godot-sync-" + [System.Guid]::NewGuid().ToString("N"))
$exitCode = 0
$statusMessage = "Godot addon copy complete."

try {
    if (-not (Test-Path -LiteralPath $sourceDir -PathType Container)) {
        throw "Sample Godot addon folder not found: $sourceDir"
    }

    $pluginConfig = Join-Path $sourceDir "plugin.cfg"
    if (-not (Test-Path -LiteralPath $pluginConfig -PathType Leaf)) {
        throw "Sample Godot addon is missing plugin.cfg: $pluginConfig"
    }

    $destParent = Split-Path -Parent $destDir
    New-Item -ItemType Directory -Path $stagingRoot | Out-Null

    $stagedAddon = Join-Path $stagingRoot "PolyPet"
    Copy-Item -LiteralPath $sourceDir -Destination $stagedAddon -Recurse -Force
    Strip-GodotUidFields -Root $stagedAddon

    New-Item -ItemType Directory -Path $destParent -Force | Out-Null
    if (Test-Path -LiteralPath $destDir) {
        Remove-Item -LiteralPath $destDir -Recurse -Force
    }

    Move-Item -LiteralPath $stagedAddon -Destination $destDir
    Write-Host "Copied sample Godot addon into $destDir"
}
catch {
    if ($exitCode -eq 0) {
        $exitCode = Get-LastExitCodeOrDefault
        if ($exitCode -eq 0) {
            $exitCode = 1
        }
    }

    $statusMessage = "Godot addon copy failed: $($_.Exception.Message)"
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
