$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
& "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" rcx $args
$env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
exit $LASTEXITCODE