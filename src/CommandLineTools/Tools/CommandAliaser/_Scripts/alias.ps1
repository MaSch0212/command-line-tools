$env:MASCH_CLT_ISSCRIPTCONTEXT = "true"
& "$PSScriptRoot\MaSch.CommandLineTools\CommandLineTools.exe" alias $args

if ($args -and $args.Length -gt 0 -and ($args[0].StartsWith("install", "OrdinalIgnoreCase") -or $args[0].StartsWith("uninstall", "OrdinalIgnoreCase"))) {
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    Write-Host "Successfully updated local Path variable." -ForegroundColor Green
}

exit $LASTEXITCODE