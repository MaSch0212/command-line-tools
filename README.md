# command-line-tools
.NET application that provides a couple of useful command line tools.


## Build Status

[![Build Status](https://masch0212.visualstudio.com/MaSch/_apis/build/status/MaSch0212.command-line-tools?branchName=main)](https://masch0212.visualstudio.com/MaSch/_build/latest?definitionId=3&branchName=main)

## Included Tools

| Name | Command | Description |
| ---- | ------- | ----------- |
| Directory Aliaser | cdx | Manages directory aliases. Aliases to directories can be added, removed and requested. <br> Example: Add path using `cdx add "my-dir" -d "C:\my\special\directory"` and navigate to it using `cdx my-dir`. |
| Command Aliaser | alias | Manages command aliases. Aliases to execute commands can be added, removed and listed. |
| Attach | attach | Attaches console output to a given process. |
| Robocopy Runner | rcx | Provides a different way to execute robocopy. Also contains some robocopy extensions (e.g. using `-P` shows a progress bar). |
| Super User | su | Elevates the current command line to administrative rights. |
| Super User Do | sudo | Executes a command in an elevated process. |