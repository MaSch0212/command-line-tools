$openFile = "$($env:TEMP)\cdx-open.tmp"
if (Test-Path $openFile -PathType Leaf) {
    Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
}

& "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" cdx $args --from-script

if (Test-Path $openFile -PathType Leaf) {
    Get-Content $openFile -Raw | Set-Location
    Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
}

exit $LASTEXITCODE