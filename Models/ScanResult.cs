namespace ModChecker.Models;

public enum Severity
{
    Error,
    Warning,
    Info
}

public class Issue
{
    public required string ModName { get; set; }
    public required string Category { get; set; }
    public Severity Severity { get; set; }
    public required string Title { get; set; }
    public string Explanation { get; set; } = "";
    public string FixSuggestion { get; set; } = "";
    public string CodeSnippet { get; set; } = "";
    public string SourceFile { get; set; } = "";
}

public class ModResult
{
    public required string FolderName { get; set; }
    public string? UniqueID { get; set; }
    public string? DisplayName { get; set; }
    public string? Version { get; set; }
    public string? MinimumApiVersion { get; set; }
    public string? ContentPackFor { get; set; }
    public bool HasManifest { get; set; }
    public bool HasContentJson { get; set; }
    public List<Issue> Issues { get; set; } = new();
}

public class ScanResult
{
    public string? GameRootPath { get; set; }
    public string? ModsPath { get; set; }
    public string? SmapiVersion { get; set; }
    public string? GameVersion { get; set; }
    public DateTime ScanTime { get; set; } = DateTime.Now;
    public long ElapsedMs { get; set; }

    public List<ModResult> Mods { get; set; } = new();
    public List<Issue> GlobalIssues { get; set; } = new();

    public int ErrorCount => CountBySeverity(Severity.Error);
    public int WarningCount => CountBySeverity(Severity.Warning);
    public int InfoCount => CountBySeverity(Severity.Info);
    public int ModCount => Mods.Count;

    private int CountBySeverity(Severity sev) =>
        GlobalIssues.Count(i => i.Severity == sev) +
        Mods.Sum(m => m.Issues.Count(i => i.Severity == sev));
}
