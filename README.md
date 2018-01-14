# dotGet [![Windows build status](https://ci.appveyor.com/api/projects/status/github/tonerdo/dotget?branch=master&svg=true)](https://ci.appveyor.com/project/tonerdo/dotget) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

dotGet is a command line installer for cross platform CLI tools built with .NET Core. dotGet makes your .NET Core command line tools, distributed via NuGet, available for use via the system path.

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

To see a list of commands, run:

```shell
dotnet get --help
```

The current commands are (output from `dotnet get --help`):

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

### Installing a tool

To install a .NET Core tool, simply run:

```shell
dotnet get install <source>[@version]
```

`source` in this case is a published NuGet package with the following structure:

```shell
├── build
├── content
├── lib
├── tools
│   └── netcoreapp*.*
│       └── <app>.deps.json
│       └── <app>.runtimeconfig.json
│       └── <app>.dll
│       └── <some-dependency>.dll
│       └── <some-other-dependency>.dll
└── <package>.nuspec
```

* The NuGet package *must* contain a `tools` folder (the other folders are completely optional for dotGet)
* The `tools` folder should have at least one sub-folder with a .NET Core App moniker (e.g `netcoreapp2.0`)
* The contents of the `netcoreapp*.*` folder should be a published [framework dependent](https://docs.microsoft.com/en-us/dotnet/core/deploying/) .NET Core app.
* The `netcoreapp*.*` folder can contain multiple framework dependent .NET Core apps.

After installation the command `<app>` will now be available from the system path and invoking that will execute the .NET Core app.

### Updating a tool

To update an already installed .NET Core tool, simply run:

```shell
dotnet get update <source>
```

### Updating dotGet

To update dotGet itself, you just need to run the `update` command without including a `<source>`, like so:

```shell
dotnet get update
```

### Listing installed tools

To get a list of all installed .NET Core tools, simply run:

```shell
dotnet get list
```

### Uninstalling a tool

To remove a .NET Core tool, you simply run:

```shell
dotnet get uninstall <source>
```

## Issues & Contributions

If you find a bug or have a feature request, please report them at this repository's issues section. Contributions are highly welcome, however, except for very small changes, kindly file an issue and let's have a discussion before you open a pull request.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.