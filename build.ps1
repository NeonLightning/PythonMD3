dotnet tool update --global MacroDeck.Plugin.Cli --prerelease
if ($LASTEXITCODE -ne 0) { dotnet tool install --global MacroDeck.Plugin.Cli --prerelease }
macrodeck-plugin build --source src/MacroDeck.PythonScript --output ./artifacts
macrodeck-plugin inspect --artifact ./artifacts/com.example.python-script-1.0.0.macroDeckPlugin
