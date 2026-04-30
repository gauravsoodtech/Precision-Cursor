param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "MouseLineLock.sln"

$programFilesX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$candidates = @(
    (Join-Path ${env:ProgramFiles} "Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"),
    (Join-Path $programFilesX86 "Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"),
    (Join-Path ${env:WINDIR} "Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe")
)

$msbuild = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $msbuild) {
    throw "MSBuild.exe was not found."
}

& $msbuild $solution /p:Configuration=$Configuration /p:Platform="Any CPU"
exit $LASTEXITCODE
