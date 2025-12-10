# TODO

* Hired mercenaries should not have the "freeded from slaver" mood modifier
    * May be able to derive a Mercenary class from Pawn which overrides the PreTrade() method which handles the mood modifier
* Make mercenaries dismissable without a mood penality from "banishment"
* Charge an upkeep for mercs?
    * Yearly starting at the first full calendar year
    * Upkeep paid at last day of the year
        * But can be delayed by one month
        * If left unpaid then merc leaves with all equipment
            * Maybe even does something more extreme...

# install/

* dotnet-cli
    * dotnet-cli requires a SDK so just install the latest for now
    * should install skd 8.0 for ilspycmd
* Mono
    * needed on Linux
    * export FrameworkPathOverride=/usr/lib/mono/4.5

# extensions/

* C# Dev Kit
    * "dotnet.backgroundAnalysis.analyzerDiagnosticsScope": "fullSolution",
    * "dotnet.backgroundAnalysis.compilerDiagnosticsScope": "fullSolution",
* Mono Debug
* ILSpy

# debugger/

Follow instructions here (use 127.0.0.1:50000):

https://github.com/pardeike/Rimworld-Doorstop

# libs/

* Copy all DLLs in "**/RimWorld/RimWorldLinux_Data/Managed" to "./SimpleMercenaries.Core/lib"
    * Delete all System.*.dll and mscorlib.dll but leave netstandard.dll (otherwise dotnet-cli likes to include them in the build output even though it shouldn't)