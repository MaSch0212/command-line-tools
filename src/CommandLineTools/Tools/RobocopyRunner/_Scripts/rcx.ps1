$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
& "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" rcx $args
exit $LASTEXITCODE