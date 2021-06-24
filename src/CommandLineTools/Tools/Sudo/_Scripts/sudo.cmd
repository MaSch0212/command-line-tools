@echo off

call "%~dp0\MaSch.CommandLineTools\CommandLineTools.exe" sudo %*
exit /B %ERRORLEVEL%