# Nexus-Embedded-Editor
Nexus Embedded Editor is a "hacky" project that attempts to
put an external editor in front of Roblox Studio's script
editor for those using file system-based projects like
[Rojo](https://github.com/rojo-rbx/rojo). The goal is to allow
for development in external editors while still being able to easily
swap to Roblox Studio and open scripts from Roblox Studio's explorer.

## Running
The server can be started by running the binary in the command line.
Assuming the binary is named `rovscode.exe`, the command would look like
the following.
```
$ ./rovscode.exe
```
It is recommended to start it in a directory or child directory with a Rojo
project.

If a custom Rojo project script is in use, it can be defined as a parameter.
```
$ ./rovscode.exe customfile.project.json
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
Each editor has their own associated plugin rather than 1 massive plugin with
all of the editors since Roblox developers using Rojo typically use only 1 external editor.
The plugins can be found on the Roblox Market Place for the following editors:
* [Microsoft Visual Studio Code](https://www.roblox.com/library/4790916291/Nexus-Embedded-VS-Code)

## Supported Project Structures
Currently, only [Rojo](https://github.com/rojo-rbx/rojo) is supported
since it is project manager endorsed by Roblox employees. Pull requests
and issues about adding other project structures are accepted if there is
a justification of why that tool is used.

## Limitations
This project has a lot of limitations that should be considered.

### Windows Only
While Rojo and a lot of external editors exist on macOS and Linux,
Nexus Embedded Editor is only buildable and usable under Windows.

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

## Potential Future Changes
### Custom Ports
It is currently possible to have more than 1 Rojo project open using
different ports, but it isn't possible currently since ports can't be changed.
This could be addressed reasonably easily on the server, but would require adding
a user interface for the client.

### More Editors
More editors could be added if there are requests for them. See the section
about supported editors.

### More Project Structures
Currently, only Rojo is supported for project structures. Alternatives to Rojo
exist and could be supported. The file resolver may need to be modified for
cases where project files are different (ex: `.client.lua` in Rojo is `.local.lua`
in another system).

## Building
Since the project uses .NET Framework and uses Windows-specific features in .NET
Framework (mainly `System.Windows.Automation`), this project can only be built
on Windows.

```
$ nuget restore NexusEmbeddedEditor/
$ msbuild.exe NexusEmbeddedEditor/
```

## Server Technical Details
More information on how the server works can be found in the
[technical details](technical-details.md) file.

## Contributing
Both issues and pull requests are accepted for this project. Issues may not be
fixable due to limitations of the system.

## License
Nexus Instance is available under the terms of the MIT 
License. See [LICENSE](LICENSE) for details.
