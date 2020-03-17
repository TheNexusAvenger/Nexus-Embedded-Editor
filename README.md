# Nexus-Embedded-Editor
Nexus Embedded Editor is a "hacky" project that attempts to
put an external editor in front of Roblox Studio's script
editor for those using file system-based projects like
[Rojo](https://github.com/rojo-rbx/rojo). The goal is to allow
for development in external editors while still being able to easily
swap to Roblox Studio and open scripts from Roblox Studio's explorer.

## Running
Assuming a pre-existing Rojo project is set up and is connected in
Roblox Studio, the server can be started by running the binary in
the command line. Assuming the binary is named `rmsvc.exe`, the command
would look like the following.
```
$ ./rmsvc.exe
```

If a custom Rojo project script is in use, it can be defined as a parameter.
```
$ ./rmsvc.exe customfile.project.json
```

In Roblox Studio with an open place and the `HttpService` enabled, there are
2 buttons that can be used:
* Connect - Connects Roblox Studio to the editor so that opening scripts in Roblox Studio opens the scripts in the editor.
* Attach - "Attaches" the editor to the Roblox Studio editor. This action implicitly connects the editor.

Note: A valid Rojo project needs to be set up, but Rojo does not need
to be running. **If Rojo is not running, changes in the editor will not
show in Roblox.**

## Supported Editors
Currently, [Microsoft Visual Studio Code](https://code.visualstudio.com/) is
supported due to its adoption by Roblox developers using Rojo. Other editors
like Sublime Text, Atom, and Notepad++ could theoretically be added. Pull requests
and issues about adding other supported editors are accepted if there is a justification
of why that editor is used. Familiarity is a valid reason unless it is an "inferior editor",
like Notepad or WordPad.

### Plugins
Each editor has their own assossiated plugin rather than 1 massive plugin with
all of the editors since Roblox developers using Rojo typically use only 1 external editor.
The plugins can be found on the Roblox Market Place for the following editors:
* [Microsoft Visual Studio Code](https://www.roblox.com/)

## Building
Since the project uses .NET Framework and uses Windows-specific features in .NET
Framework (mainly `System.Windows.Automation`), this project can only be built
on Windows.

### With .NET Core Installed
If .NET Core is installed and `dotnet` is available, the project can be built using:
```
$ dotnet build NexusEmbeddedEditor/
```

### Without .NET Core Installed
Assuming Microsoft Visual Studio 2019 (Community Edition) is installed, the project can be built using:
```
$ C:/Program\ Files\ \(x86\)/Microsoft\ Visual\ Studio/2019/Community/MSBuild/Current/Bin/msbuild.exe NexusEmbeddedEditor/
```

## Contributing
Both issues and pull requests are accepted for this project. Issues may not be
fixable due to limitations of the system.

## Limitations
This project has a lot of limitations that should be considered.

### Winodws Only
While Rojo and a lot of external editors exist on macOS and Linux,
Nexus Embedded Editor is only buildable and usable under Windows.

### Delays When Opening Scripts
When opening scripts in Roblox Studio, there is a delay (<250ms, >100ms)
between opening a script as a tab and the script opening in the external
editor. This is a limitation of searching through and reading objects in
`System.Windows.Automation`.

### Editor Disappears When Interacting With Studio
When using widgets in Roblox Studio, the external editor will disappear
until the Roblox Studio editor is focused by either changing the tab or
clicking on it. Since the program overlays the external editor as a normal
window, focusing on Roblox Studio will force it behind. While it is possible
to make it refocus in certain cases like resizing, the fix resulted in pressing
buttons re-focusing the external editor.

### No Keyboard Shortcut Passthrough
Since Roblox Studio is not focused, any keyboard shortcuts for Roblox will
not work with the external editor focused. Shortcut passthrough is possible but
would result in unwanted shortcuts being passed through to Roblox Studio.

## License
Nexus Instance is available under the terms of the Apache 
Liscence 2.0. See [LICENSE](LICENSE) for details.