param (
    [Parameter(Mandatory = $true)]
    [string] $PublishDir,
    [Parameter(Mandatory = $true)]
    [string] $ProjectDir
)

Write-Host "CurrentDir : $((Get-Location).Path)"
Write-Host "PublishDir : $PublishDir"
Write-Host "ProjectDir : $ProjectDIr"

$subDirName = "MaSch.CommandLineTools"
$subDir = Join-Path $PublishDir $subDirName
$metaDir = Join-Path $PublishDir $metaDirName

Remove-Item -Path $subDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path $subDir -ItemType Directory -Force
New-Item -Path $metaDir -ItemType Directory -Force

Move-Item -Path (Join-Path $PublishDir "*") -Destination $subDir -Exclude $subDirName, *.cmd, *.ps1

$scriptsDir = Join-Path $ProjectDir "Scripts"

Copy-Item -Path (Join-Path $scriptsDir "*") -Destination $PublishDir -Include *.cmd, *.ps1 -Force