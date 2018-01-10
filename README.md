[![Windows build status](https://ci.appveyor.com/api/projects/status/github/tonerdo/dotget?branch=master&svg=true)](https://ci.appveyor.com/project/tonerdo/dotget)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
# dotGet

dotGet is a tool intended to make it easy to distribute cross platform CLI tools built with .NET Core. All tools are installed globally (project independent) and their exported commands are made accessible from the platform specific terminal/console.

## Installation

### Prerequisites

Install [.NET Core](https://www.microsoft.com/net/download/core)

### Windows

```powershell
(new-object Net.WebClient).DownloadString("https://raw.githubusercontent.com/tonerdo/dotget/master/scripts/install.ps1") | iex
```

### Unix

```shell
curl -s https://raw.githubusercontent.com/tonerdo/dotget/master/scripts/install.sh | bash
```
_Note: Ensure that $HOME/.dotget/bin has been added to your path after installation_

## Usage

```shell
.NET Core Tools Global Installer 1.0.0.0

Usage: dotnet get [options] [command]

Options:
  -h|--help     Show help information
  -v|--version  Show version information
  --verbose     Enable verbose output

Commands:
  install    Installs a .NET Core tool
  list       Lists all installed .NET Core tools
  uninstall  Uninstalls a .NET Core tool
  update     Updates a .NET Core tool

Use "dotnet get [command] --help" for more information about a command.
```

## Issues & Contributions

If you find a bug or have a feature request, please report them at this repository's issues section. Contributions are highly welcome, however, except for very small changes, kindly file an issue and let's have a discussion before you open a pull request.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.