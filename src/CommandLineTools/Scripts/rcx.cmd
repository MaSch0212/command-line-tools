@echo off

call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" rcx %*
exit /B %ERRORLEVEL%