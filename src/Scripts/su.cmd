@echo off

call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" su %*
exit /B %ERRORLEVEL%