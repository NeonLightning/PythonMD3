using MacroDeck.Localization;
using MacroDeck.Sdk.Actions;
using Serilog;

namespace MacroDeck.PythonScript;

public sealed class RunPythonScriptAction(ILogger logger) : IActionDefinition
{
    public string Id => "run-python-script";

    public LocalizedText Name => "Run Python script";

    public LocalizedText Description =>
        "Runs a Python script when the button is pressed. Does not wait for it to finish.";

    public IReadOnlyList<ActionParameter> Parameters { get; } =
    [
        ActionParameter.File("script", label: "Script path", description: "Full path to the .py file", required: true),
        ActionParameter.Text("args", label: "Arguments", description: "Optional. Quote values containing spaces.", required: false),
        ActionParameter.File("python", label: "Python path", description: "Optional. Leave empty to use py / python3 from PATH.", required: false),
        ActionParameter.Folder("workdir", label: "Working directory", description: "Optional. Defaults to the script's folder.", required: false),
    ];

    public IActionExecutor CreateExecutor() => new Executor(logger);

    private sealed class Executor(ILogger logger) : IActionExecutor
    {
        public Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            string Get(string key) => context.Parameters.GetValueOrDefault(key)?.ToString() ?? "";

            var error = PythonRunner.Start(
                new PythonRunner.Request(Get("script"), Get("args"), Get("python"), Get("workdir")),
                logger);

            return error is null
                ? ActionResult.SucceededTask
                : Task.FromResult(ActionResult.Failed(ActionErrorCodes.InvalidParameter, error));
        }
    }
}
