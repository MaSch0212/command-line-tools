$rawArgs = $MyInvocation.Line.Substring($MyInvocation.InvocationName.Length + 1)
$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
try
{
    if ($args[0] -eq "please") {
        $command = (Get-History | Select-Object -Last 1).CommandLine
        if (-not [String]::IsNullOrEmpty($command)) {
            $rawArgs = $rawArgs.Substring(6)
            Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" sudo $rawArgs -- $command"
            exit $LASTEXITCODE
        }
    }

    Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" sudo $rawArgs"
    exit $LASTEXITCODE
}
finally
{
    $env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
}