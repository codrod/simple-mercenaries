# Simple Mercenaries - RimwWorld Mod

"Simple Mercenaries" is a mod for the popular game RimWorld made by [Ludeon Studios](https://ludeon.com/blog/). This mod adds mercenaries which can be hired through a simple UI addition integrated into the standard in-game "Comms Console".

## Contributing

Contributions are welcome just follow the [Development Environment Setup](#development-environment-setup) section below and then submit a pull-request to the repository when finished. No need to get into contact with me first but I may have some questions so checking for messages on Github is helpful. Also I can't guarantee I will notice or respond to the pull-request in a timely manner.

## Development Environment Setup

This section will walk you through the process of setting up [VS Code](https://code.visualstudio.com/) to begin development on the repository. Note that you don't have to use VS Code but these instructions only cover the setup process for VS Code.

### Dependencies

This repo uses the dotnet-cli for its build system so it will need to be installed first.

* dotnet-cli
    * Installing any .NET Core SDK should include the dotnet-cli but you may need to install it separately depending on your system
    * If you are on Windows then you should also installed the .NET 4.7.2 SDK
* Mono
    * If you are developing on Linux then you will need to install Mono which should be available through your package manager
        * Note you most likely want to install the mono-complete (or similar) package
    * Once Mono is installed add the following to your .bashrc (or equivalent) so the dotnet-cli will actually use Mono
        * export FrameworkPathOverride=/usr/lib/mono/4.5
        * Note the above path may be slightly different depending on you system

### Extensions

If you are using VS Code then the following extensions are strongly recommended.

* C# Dev Kit
    * Note by default "C# Dev Kit" only validates currently opened files so I recommend you add the following to your settings config
        * "dotnet.backgroundAnalysis.analyzerDiagnosticsScope": "fullSolution",
        * "dotnet.backgroundAnalysis.compilerDiagnosticsScope": "fullSolution",
* Mono Debug
* ilspy-vscode

### Cloning

This solution is designed to be developed directly inside the RimWorld mods folder for simplicity.

1. Clone this repo into the "**/RimWorld/Mods" folder
2. Copy all DLLs in "**/RimWorld/RimWorldLinux_Data/Managed" to the "./SimpleMercenaries.Core/lib" folder
3. Delete all System.*.dll and mscorlib.dll but leave netstandard.dll
    * Otherwise dotnet-cli likes to include them in the build output even though it shouldn't

### Building

This project uses a standard dotnet-cli build process so basic familiarity with [dotnet](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet) is helpful.

1. run 'dotnet build'
    * This will build the entire project and output the final DLL to the Assemblies folder for the latest supported version

### Debugging

Since RimWorld is a Unity project you will need to use [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) (or an equivalent) to attach a debugger at run-time.

1. Follow instructions [here](https://github.com/pardeike/Rimworld-Doorstop) to setup the RimWold specific Unity Doorstop plugin
    * Note when configuring the debug server use 127.0.0.1:50000
2. Execute the run script provided by the RimWorld Unity Doorstop plugin
    * Note if you are on Linux a shortcut "./run.sh" script is included for convenience
3. In VS Code select "Run -> Start Debugging" to launch the debugger

## License

This project is licensed under the MIT License - see the LICENSE.md file for details