$rawArgs = $MyInvocation.Line.Substring($MyInvocation.InvocationName.Length + 1)
$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
try {
    Invoke-Expression "& `"$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe`" alias $rawArgs"

    if ($rawArgs -and ($rawArgs.StartsWith("install", "OrdinalIgnoreCase") -or $rawArgs.StartsWith("uninstall", "OrdinalIgnoreCase"))) {
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        Write-Host "Successfully updated local Path variable." -ForegroundColor Green
    }

    exit $LASTEXITCODE
}
finally {
    $env:MASCH_CLT_ISSCRIPTCONTEXT = "false"
}