param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot "build.ps1") -Configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$testExe = Join-Path $root "tests\MouseLineLock.Tests\bin\$Configuration\MouseLineLock.Tests.exe"
if (-not (Test-Path -LiteralPath $testExe)) {
    throw "Test executable was not found: $testExe"
}

& $testExe
exit $LASTEXITCODE
