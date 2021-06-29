@echo off

set MASCH_CLT_ISSCRIPTCONTEXT=true
call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" alias %*

set args=%*
if /i "%args:~0,7%"=="install" goto updatepath
if /i "%args:~0,9%"=="uninstall" goto updatepath

:end
exit /B %ERRORLEVEL%

:updatepath
for /f "tokens=2*" %%A in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v Path') do set syspath=%%B
for /f "tokens=2*" %%A in ('reg query "HKCU\Environment" /v Path') do set userpath=%%B
set PATH=%syspath%;%userpath%
echo [92mSuccessfully updated local Path variable.[0m
goto end