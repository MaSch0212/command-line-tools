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
$metaDirName = ".meta"
$subDir = Join-Path $PublishDir $subDirName
$metaDir = Join-Path $PublishDir $metaDirName

Remove-Item -Path $subDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path $subDir -ItemType Directory -Force
New-Item -Path $metaDir -ItemType Directory -Force

Move-Item -Path (Join-Path $PublishDir "*.pdb"), (Join-Path $PublishDir "*.xml") -Destination $metaDir
Move-Item -Path (Join-Path $PublishDir "*") -Destination $subDir -Exclude $subDirName, $metaDirName, *.cmd, *.ps1
#Remove-Item -Path (Join-Path $subDir "*") -Recurse -Exclude CommandLineTools.exe, CommandLineTools.pdb

$scriptsDir = Join-Path $ProjectDir "Scripts"

Copy-Item -Path (Join-Path $scriptsDir "*") -Destination $PublishDir -Include *.cmd, *.ps1 -Force