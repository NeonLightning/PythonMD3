using MacroDeck.Plugin.Serilog;
using MacroDeck.Plugin.Hosting;
using MacroDeck.PythonScript;

var plugin = MacroDeckPlugin.CreatePlugin(args)
    .UseMacroDeckLogging()
    .RegisterIntegration<PythonScriptIntegration>()
    .Build();

await plugin.RunAsync();
