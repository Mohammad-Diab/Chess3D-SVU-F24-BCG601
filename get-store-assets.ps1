# Copies the Unity Asset Store packs this project needs into Assets\.
# They live in the private repo Chess3D-Store-Assets (their license forbids publishing them),
# so this works only for an account with access to it. They are git-ignored here: never commit them.
#
#   powershell -ExecutionPolicy Bypass -File .\get-store-assets.ps1

$ErrorActionPreference = 'Stop'
$repo = 'https://github.com/Mohammad-Diab/Chess3D-Store-Assets.git'
$packs = 'PBR Chess Pack Vol-1 (Simple)', 'Stylized Wood Textures'
$assets = Join-Path $PSScriptRoot 'Assets'
$temp = Join-Path ([IO.Path]::GetTempPath()) ('chess3d-store-assets-' + [guid]::NewGuid().ToString('N').Substring(0, 8))

git clone --depth 1 $repo $temp
if ($LASTEXITCODE -ne 0) { throw "Could not clone $repo (no access?)" }
try {
    foreach ($pack in $packs) {
        $target = Join-Path $assets $pack
        if (Test-Path -LiteralPath $target) { Write-Host "Already there, skipped: $pack"; continue }
        Copy-Item -LiteralPath (Join-Path $temp $pack) -Destination $target -Recurse
        Write-Host "Added: $pack"
    }
}
finally {
    Remove-Item -LiteralPath $temp -Recurse -Force
}
