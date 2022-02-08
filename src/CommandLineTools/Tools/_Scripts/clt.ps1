$rawArgs = $MyInvocation.Line.Substring($MyInvocation.InvocationName.Length + 1)
Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" $rawArgs"
exit $LASTEXITCODE