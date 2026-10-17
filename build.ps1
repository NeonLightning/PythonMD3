dotnet tool update --global MacroDeck.Plugin.Cli --prerelease
if ($LASTEXITCODE -ne 0) { dotnet tool install --global MacroDeck.Plugin.Cli --prerelease }
macrodeck-plugin build --source src/MacroDeck.PythonScript --output ./artifacts --force
macrodeck-plugin inspect --artifact ./artifacts/com.example.python-script-*.macroDeckPlugin
