$rawArgs = $MyInvocation.Line.Substring($MyInvocation.InvocationName.Length + 1)
$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
try {
	Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" rcx $rawArgs"
	exit $LASTEXITCODE
}
finally {
	$env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
}