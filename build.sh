#!/usr/bin/env bash
set -e
dotnet tool update --global MacroDeck.Plugin.Cli --version 3.0.0-beta.17 || dotnet tool install --global MacroDeck.Plugin.Cli --version 3.0.0-beta.17
macrodeck-plugin build --source src/MacroDeck.PythonScript --output ./artifacts --force
macrodeck-plugin inspect --artifact ./artifacts/com.neonlightning.python-script-*.macroDeckPlugin