using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Localization;
using Serilog;

namespace MacroDeck.PythonScript;

public sealed class PythonScriptIntegration : IPluginIntegration
{
    public PythonScriptIntegration(ILogger logger)
    {
        Actions = [new RunPythonScriptAction(logger)];
    }

    public IReadOnlyList<IActionDefinition> Actions { get; }

    public Task InitializeAsync(IIntegrationContext context) => Task.CompletedTask;

    public Task ShutdownAsync() => Task.CompletedTask;
}
