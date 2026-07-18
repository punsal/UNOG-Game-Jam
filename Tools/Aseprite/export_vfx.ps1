# Runs the Last Light VFX Aseprite pipeline: templates -> validate -> export.
# Usage: Tools/Aseprite/export_vfx.ps1 [-Aseprite <path>]
param([string]$Aseprite = "")

$Dir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not $Aseprite) {
    $candidates = @(
        (Get-Command aseprite -ErrorAction SilentlyContinue).Source,
        "C:\Program Files\Aseprite\Aseprite.exe",
        "C:\Program Files (x86)\Steam\steamapps\common\Aseprite\Aseprite.exe"
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c)) { $Aseprite = $c; break }
    }
}

if (-not $Aseprite) {
    Write-Error "aseprite binary not found. Pass -Aseprite <path>."
    exit 1
}

Write-Host "Using aseprite: $Aseprite"
& $Aseprite -b --script "$Dir\create_vfx_templates.lua"; if ($LASTEXITCODE) { exit $LASTEXITCODE }
& $Aseprite -b --script "$Dir\validate_vfx_palette.lua"; if ($LASTEXITCODE) { exit $LASTEXITCODE }
& $Aseprite -b --script "$Dir\export_vfx.lua"; if ($LASTEXITCODE) { exit $LASTEXITCODE }
Write-Host "VFX export pipeline complete. Re-import in Unity to pick up new sheets."
