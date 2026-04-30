param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "DpiAssistant.sln"

$programFilesX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$programFiles = [Environment]::GetFolderPath("ProgramFiles")
$pathMsBuild = Get-Command "MSBuild.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source
$vswhere = Join-Path $programFilesX86 "Microsoft Visual Studio\Installer\vswhere.exe"
$vswhereMsBuild = $null

if (Test-Path -LiteralPath $vswhere) {
    $vswhereMsBuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
}

$candidates = New-Object "System.Collections.Generic.List[string]"

function Add-Candidate {
    param([string]$Path)

    if (-not [string]::IsNullOrWhiteSpace($Path)) {
        [void]$candidates.Add($Path)
    }
}

Add-Candidate $env:MSBUILD_EXE_PATH
Add-Candidate $pathMsBuild
Add-Candidate $vswhereMsBuild
Add-Candidate (Join-Path $programFiles "Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe")
Add-Candidate (Join-Path $programFiles "Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe")
Add-Candidate (Join-Path $programFiles "Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe")
Add-Candidate (Join-Path $programFilesX86 "Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe")
Add-Candidate (Join-Path $programFilesX86 "Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe")
Add-Candidate (Join-Path ${env:WINDIR} "Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe")
Add-Candidate (Join-Path ${env:WINDIR} "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")

$msbuild = $null
foreach ($candidate in $candidates) {
    if (Test-Path -LiteralPath $candidate) {
        $msbuild = $candidate
        break
    }
}

if (-not $msbuild) {
    throw "MSBuild.exe was not found."
}

& $msbuild $solution /p:Configuration=$Configuration /p:Platform="Any CPU"
exit $LASTEXITCODE
