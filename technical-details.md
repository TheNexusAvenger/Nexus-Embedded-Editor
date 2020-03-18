# Technical-Details
This covers the technical details of how Nexus Embedded Editor
works.

Before going over how Nexus Embedded Editor works, it
is important to go over what it can and can't do. To
start it can:
* Determine the dimensions of Roblox Studio's editor and if it is open.
* Resizes and moves external editors on top of Roblox Studio's editor.
* Determine the name of active Studio tab.
* Read Roblox Studio's editor.

But it can't:
* Seamlessly embed another editor as if it was part of the application.
* Directly transfer information between Roblox Studio and an external editor.

While the name implies it embeds an external editor in Roblox Studio,
it is just an overlay with some integration rather than seamlessly embedding.

## Connecting vs Attaching
Nexus Embedded Editor can have 2 effective states, which are Connected and
Attached. If an external editor is connected, opening windows in Roblox Studio
will result in them being opened in the external editor. If an external editor is
attached, it is connected but is also moved in front of the Roblox Editor. If the
external editor is only connected, all the steps except  `Moving The External Editor`
are done.

## `System.Windows.Automation` in .NET Framework
.NET Framework for Windows includes support for the `System.Windows.Automation`
namespace. This namespace includes classes for reading user interface elemenets,
as well as manipulate them. This namespace exists to automate user interfaces for
tasks like automated testing. This is used for several tasks in the system.

## Fetching Windows
To be able to read and manipulate windows, the windows to read and manipulate
first need to be found.

### Fetching Roblox Studio
Roblox Studio can be opened multiple times in a single system. A property that Roblox
has is the window name will be "tab name - Roblox Studio", where the tab name is the
active tab in Roblox Studio. It can be either the viewport, or it could be a script.
This property is exploited for the initial fetching of the correct Roblox Studio window
by creating and opening a script with a unique name generated from
[`HttpService:GenerateGUID()`](https://developer.roblox.com/en-us/api-reference/function/HttpService/GenerateGUID)
and sending a message to the server to look for the window with the title containing the
random string.

### Fetching External Editors
The actual method for fetching windows is up to the implementation of the external editor.
For Microsoft Visual Studio Code opened in a directory, the name for the window follows
"tab name - project directory - Visual Studio Code", where the project directory is the
directory that was opened with the `Code.exe` executable.

## Getting Roblox Studio's Editor's Bounding Box
### Getting The Editor
After the Roblox Studio window is found, it is possible to search through the elements
of the window for find specific elements given conditions. With the Roblox Studio window
as the root, editors windows can be located with `Qt5QWindowIcon[ControlType:=ControlType.Pane]
,Qt5QWindowIcon[ControlType:=ControlType.Pane],Qt5QWindowIcon[ControlType:=ControlType.Pane],
Qt5QWindowIcon[ControlType:=ControlType.Edit]`. Each opened script has an edit window, meaning
there is between 0 and many of the editors. Finding the last open editor to base the bounding box
is used by checking for `HasKeyboardFocus` to be `true`.

### Getting The Bounding Size
With the editor defined, a `Rect` struct representing the bounding size can be found by
reading the `BoundingRectangle` of the editor window. Included is the screen position
and the size in pixels.

## Moving The External Editor
In `System.Windows.Automation`, "Patterns" are used to read and modify attributes that aren't
guarenteed to be supported by an element. For example, a text box can support writing text
but can't support being minimized. To move windows, the `WindowPattern` is needed to set the
visual state and the `TransformPattern` is needed to move it. If the `WindowPattern` is supported,
it can be used to set the visual state of a window to, `Minimized`, `Normal`, or `Maximized`. In
this case, the external editor is minimized if Roblox Studio is minimized (read from the `WindowPattern`
of Roblox studio) and is set to normal for when a script is opened and is attached. `Move` and `Resize`
are used from the `TransformPattern` to move the external editor to the bounding box of the Roblox
Studio editor.

## Opening Scripts
Opening scripts is currently very complex since it involves finding the current active script
and attempting to resolve the opened file using the script from the `Name` of the tab, and using
the script source using the `Value` attribute of the `ValuePattern` of the editor if more than
one exists. After a file is found, it is up to the editor's implementation to open it. For Microsoft
Visual Studio Code, opening a file using the `Code.exe` executable will use an existing window
if one exists.

## Replicating Changes Back to Roblox Studio
Nexus Embedded Editor does not handle replicating scripts back to Roblox Studio since it up
to the project manager to do it (like Rojo).