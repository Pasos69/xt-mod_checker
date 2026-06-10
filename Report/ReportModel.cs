using ModChecker.Models;

namespace ModChecker.Report;

/// <summary>报告聚合逻辑</summary>
public static class ReportModel
{
    /// <summary>汇总扫描结果为统一报告视图</summary>
    public static ReportView Aggregate(ScanResult scan)
    {
        var view = new ReportView
        {
            ScanTime = scan.ScanTime,
            ElapsedMs = scan.ElapsedMs,
            ModsPath = scan.ModsPath ?? "",
            GameRootPath = scan.GameRootPath ?? "",
            SmapiVersion = scan.SmapiVersion ?? "",
            GameVersion = scan.GameVersion ?? "",
            ModCount = scan.ModCount,
            ErrorCount = scan.ErrorCount,
            WarningCount = scan.WarningCount,
            InfoCount = scan.InfoCount
        };

        // 按严重级别分组的全局问题
        view.Errors.AddRange(scan.GlobalIssues.Where(i => i.Severity == Severity.Error));
        view.Warnings.AddRange(scan.GlobalIssues.Where(i => i.Severity == Severity.Warning));
        view.Infos.AddRange(scan.GlobalIssues.Where(i => i.Severity == Severity.Info));

        // 每个模组的问题
        foreach (var mod in scan.Mods)
        {
            view.Errors.AddRange(mod.Issues.Where(i => i.Severity == Severity.Error));
            view.Warnings.AddRange(mod.Issues.Where(i => i.Severity == Severity.Warning));
            view.Infos.AddRange(mod.Issues.Where(i => i.Severity == Severity.Info));

            // 模组摘要（per-mod）
            if (mod.Issues.Count > 0)
            {
                var summary = new ModSummary
                {
                    DisplayName = mod.DisplayName ?? mod.FolderName,
                    UniqueID = mod.UniqueID ?? "",
                    FolderName = mod.FolderName,
                    Version = mod.Version ?? "",
                    Issues = mod.Issues.ToList()
                };
                view.ModSummaries.Add(summary);
            }
        }

        // 完整模组表
        view.ModTable.AddRange(scan.Mods.Select(m => new ModTableRow
        {
            DisplayName = m.DisplayName ?? m.FolderName,
            UniqueID = m.UniqueID ?? "-",
            Version = m.Version ?? "-",
            FolderName = m.FolderName,
            HasManifest = m.HasManifest,
            HasContentJson = m.HasContentJson,
            ErrorCount = m.Issues.Count(i => i.Severity == Severity.Error),
            WarningCount = m.Issues.Count(i => i.Severity == Severity.Warning),
            InfoCount = m.Issues.Count(i => i.Severity == Severity.Info)
        }));

        return view;
    }
}

public class ReportView
{
    public DateTime ScanTime { get; set; }
    public long ElapsedMs { get; set; }
    public string ModsPath { get; set; } = "";
    public string GameRootPath { get; set; } = "";
    public string SmapiVersion { get; set; } = "";
    public string GameVersion { get; set; } = "";
    public int ModCount { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }

    public List<Issue> Errors { get; set; } = new();
    public List<Issue> Warnings { get; set; } = new();
    public List<Issue> Infos { get; set; } = new();
    public List<ModSummary> ModSummaries { get; set; } = new();
    public List<ModTableRow> ModTable { get; set; } = new();
}

public class ModSummary
{
    public string DisplayName { get; set; } = "";
    public string UniqueID { get; set; } = "";
    public string FolderName { get; set; } = "";
    public string Version { get; set; } = "";
    public List<Issue> Issues { get; set; } = new();
}

public class ModTableRow
{
    public string DisplayName { get; set; } = "";
    public string UniqueID { get; set; } = "";
    public string Version { get; set; } = "";
    public string FolderName { get; set; } = "";
    public bool HasManifest { get; set; }
    public bool HasContentJson { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
}
