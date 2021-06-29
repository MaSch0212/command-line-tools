$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
try
{
    if ($args[0] -eq "please") {
        $command = (Get-History | Select-Object -Last 1).CommandLine
        if (-not [String]::IsNullOrEmpty($command)) {
            & "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" sudo $args --command $command
            exit $LASTEXITCODE
        }
    }

    & "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" sudo $args
    exit $LASTEXITCODE
}
finally
{
    $env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
}