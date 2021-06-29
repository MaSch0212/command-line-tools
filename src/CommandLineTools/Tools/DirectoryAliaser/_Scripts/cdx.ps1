$openFile = "$($env:TEMP)\cdx-open.tmp"
if (Test-Path $openFile -PathType Leaf) {
    Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
}

$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
& "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" cdx $args
$env:MASCH_CLT_ISSCRIPTCONTEXT = "false"

if (Test-Path $openFile -PathType Leaf) {
    Get-Content $openFile -Raw | Set-Location
    Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
}

exit $LASTEXITCODE