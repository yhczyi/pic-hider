#requires -Version 5.1
<#
.SYNOPSIS
    打包 PicHider 的 GUI 与 CLI 独立 exe。

.DESCRIPTION
    以自包含单文件方式发布，产物无需目标机器安装 .NET。
    输出到 output/releases，命名规则：PicHider-<gui|cli>-<arch>.exe。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/build-releases.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/build-releases.ps1 -Architectures win-x64
#>
[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string[]]$Architectures = @('win-x64'),

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$releasesDir = Join-Path $root 'output/releases'
$publishRoot = Join-Path $root 'output/publish'
New-Item -ItemType Directory -Force -Path $releasesDir | Out-Null

$targets = @(
    @{ Kind = 'gui'; Project = 'src/PicHider.App/PicHider.App.csproj'; Exe = 'PicHider.App.exe' }
    @{ Kind = 'cli'; Project = 'src/PicHider.Cli/PicHider.Cli.csproj'; Exe = 'PicHider.Cli.exe' }
)

$cleanupFailures = New-Object System.Collections.Generic.List[string]

Push-Location $root
try {
    foreach ($arch in $Architectures) {
        foreach ($target in $targets) {
            $outDir = Join-Path $publishRoot "$($target.Kind)-$arch"
            Write-Host "==> 发布 $($target.Kind) / $arch" -ForegroundColor Cyan

            dotnet publish $target.Project `
                -c $Configuration `
                -r $arch `
                --self-contained true `
                -p:PublishSingleFile=true `
                -p:IncludeNativeLibrariesForSelfExtract=true `
                -p:EnableCompressionInSingleFile=true `
                -p:DebugType=None `
                -o $outDir

            if ($LASTEXITCODE -ne 0) {
                throw "发布失败：$($target.Kind) / $arch（退出码 $LASTEXITCODE）"
            }

            $sourceExe = Join-Path $outDir $target.Exe
            if (-not (Test-Path $sourceExe)) {
                throw "未找到发布产物：$sourceExe"
            }

            $destExe = Join-Path $releasesDir "PicHider-$($target.Kind)-$arch.exe"
            Copy-Item -Path $sourceExe -Destination $destExe -Force
            Write-Host "    -> $destExe" -ForegroundColor Green

            try {
                Remove-Item -LiteralPath $outDir -Recurse -Force -ErrorAction Stop
            }
            catch {
                $cleanupFailures.Add($outDir)
            }
        }
    }
}
finally {
    Pop-Location
}

if (Test-Path -LiteralPath $publishRoot) {
    try {
        Remove-Item -LiteralPath $publishRoot -Recurse -Force -ErrorAction Stop
    }
    catch {
        $cleanupFailures.Add($publishRoot)
    }
}

foreach ($dir in ($cleanupFailures | Select-Object -Unique)) {
    Write-Warning "未能清理中转目录（可能被其他进程占用）：$dir"
}

Write-Host ''
Write-Host "发布完成，产物位于：$releasesDir" -ForegroundColor Green
Get-ChildItem -Path $releasesDir -Filter '*.exe' |
    Sort-Object Name |
    ForEach-Object { Write-Host ("  {0}  ({1:N1} MB)" -f $_.Name, ($_.Length / 1MB)) }

Read-Host "按 Enter 退出..."
