$openFile = "$($env:TEMP)\cdx-open.tmp"
if (Test-Path $openFile -PathType Leaf) {
    Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
}

$rawArgs = $MyInvocation.Line.Substring($MyInvocation.InvocationName.Length + 1)
$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
try {
    Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" cdx $rawArgs"

    if (Test-Path $openFile -PathType Leaf) {
        Get-Content $openFile -Raw | Set-Location
        Remove-Item $openFile -Force -ErrorAction Ignore | Out-Null
    }

    exit $LASTEXITCODE
}
finally {
    $env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
}