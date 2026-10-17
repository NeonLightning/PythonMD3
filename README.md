# Python Script Runner (Macro Deck 3 plugin)

Adds a **Run Python script** action. Put it on a button, point it at a `.py` file, press the button.

Parameters: script path (required), arguments, Python path, working directory.
The script is launched without a shell and is not waited on; its stdout/stderr go to the plugin log in
Macro Deck's log viewer.

## Build the installable package
Requires the .NET 10 SDK.

    ./build.sh        # macOS / Linux   (or .\build.ps1 on Windows)

This produces `artifacts/com.example.python-script-1.0.0.macroDeckPlugin`.
Drag it into the Macro Deck 3 window to install.

Before sharing, change `id`, `publisher.name` and `version` in `src/MacroDeck.PythonScript/manifest.json`
(and the file name in the build script).

## If the build complains
Macro Deck 3's SDK is in preview. If `RunPythonScriptAction.cs` doesn't compile, generate a fresh template
(`dotnet new install MacroDeck.Plugin.Templates@*-*` then `dotnet new macrodeck-plugin -n Test`),
copy the shape of its `LogMessageAction.cs`, and keep `PythonRunner.cs` as is.

## Security
The action runs whatever script the button is configured with, with your user's permissions.
Only configure scripts you trust.
