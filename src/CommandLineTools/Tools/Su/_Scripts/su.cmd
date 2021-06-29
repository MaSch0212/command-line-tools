@echo off

set MASCH_CLT_ISSCRIPTCONTEXT=true
call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" su %*
exit /B %ERRORLEVEL%