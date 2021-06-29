@echo off

set openfile=%temp%\cdx-open.tmp
del "%openfile%" >nul 2>&1

set MASCH_CLT_ISSCRIPTCONTEXT=true
call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" cdx %*
set MASCH_CLT_ISSCRIPTCONTEXT=false

if exist "%openfile%" (
    for /f "delims=" %%A in (%openfile%) do cd /d "%%A"
    del "%openfile%" >nul 2>&1
)

exit /B %ERRORLEVEL%