using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Serilog;

namespace MacroDeck.PythonScript;

public static class PythonRunner
{
    public sealed record Request(string Script, string Arguments, string? PythonPath, string? WorkingDirectory);

    private static readonly string[] ScrubbedEnv =
    [
        "LD_LIBRARY_PATH", "LD_PRELOAD", "PYTHONHOME", "PYTHONPATH",
        "APPDIR", "APPIMAGE", "ARGV0", "OWD",
    ];

    private const int QuickFailWindowMs = 2000;

    public static string? Start(Request req, ILogger log)
    {
        if (string.IsNullOrWhiteSpace(req.Script))
            return "No script path given.";
        if (!File.Exists(req.Script))
            return $"Script not found: {req.Script}";

        var workDir = !string.IsNullOrWhiteSpace(req.WorkingDirectory)
            ? req.WorkingDirectory!
            : Path.GetDirectoryName(Path.GetFullPath(req.Script))!;
        if (!Directory.Exists(workDir))
            return $"Working directory not found: {workDir}";

        string? lastError = null;
        foreach (var exe in Candidates(req.PythonPath))
        {
            var psi = new ProcessStartInfo(exe)
            {
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var name in ScrubbedEnv)
                psi.Environment.Remove(name);

            if (Path.GetFileNameWithoutExtension(exe).Equals("py", StringComparison.OrdinalIgnoreCase))
                psi.ArgumentList.Add("-3");
            psi.ArgumentList.Add("-u");
            psi.ArgumentList.Add(req.Script);
            foreach (var a in SplitArgs(req.Arguments))
                psi.ArgumentList.Add(a);

            try
            {
                var stderrTail = new ConcurrentQueue<string>();
                var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                proc.OutputDataReceived += (_, e) => { if (e.Data is not null) log.Information("[py] {Line}", e.Data); };
                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is null) return;
                    log.Warning("[py] {Line}", e.Data);
                    stderrTail.Enqueue(e.Data);
                    while (stderrTail.Count > 8) stderrTail.TryDequeue(out var dropped);
                };
                proc.Exited += (_, _) => log.Information("Script {Script} exited", req.Script);

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                log.Information("Started {Python} {Script} (pid {Pid})", exe, req.Script, proc.Id);

                if (proc.WaitForExit(QuickFailWindowMs))
                {
                    proc.WaitForExit(); // flush the async output readers
                    log.Information("Script {Script} exited with code {Code}", req.Script, proc.ExitCode);
                    if (proc.ExitCode != 0)
                    {
                        var tail = string.Join(" | ", stderrTail);
                        return $"Script exited with code {proc.ExitCode}" + (tail.Length > 0 ? $": {tail}" : ".");
                    }
                }
                return null;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                lastError = $"Could not start '{exe}': {ex.Message}";
            }
        }
        return lastError ?? "No Python interpreter found. Set the Python path parameter.";
    }

    private static IEnumerable<string> Candidates(string? explicitPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath)) { yield return explicitPath!; yield break; }
        if (OperatingSystem.IsWindows()) { yield return "py"; yield return "python"; }
        else { yield return "python3"; yield return "python"; }
    }

    public static List<string> SplitArgs(string? input)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(input)) return result;
        var sb = new StringBuilder();
        char quote = '\0';
        var has = false;
        foreach (var c in input)
        {
            if (quote != '\0')
            {
                if (c == quote) quote = '\0'; else sb.Append(c);
            }
            else if (c is '"' or '\'') { quote = c; has = true; }
            else if (char.IsWhiteSpace(c))
            {
                if (has || sb.Length > 0) { result.Add(sb.ToString()); sb.Clear(); has = false; }
            }
            else sb.Append(c);
        }
        if (has || sb.Length > 0) result.Add(sb.ToString());
        return result;
    }
}
