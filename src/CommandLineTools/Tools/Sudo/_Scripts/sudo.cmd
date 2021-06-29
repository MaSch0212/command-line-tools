@echo off

set MASCH_CLT_ISSCRIPTCONTEXT=true
call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" sudo %*
set MASCH_CLT_ISSCRIPTCONTEXT=false
exit /B %ERRORLEVEL%