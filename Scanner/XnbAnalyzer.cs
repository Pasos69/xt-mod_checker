using ModChecker.Models;

namespace ModChecker.Scanner;

/// <summary>XNB 文件分析（暂为桩，v3.1 实现）</summary>
public static class XnbAnalyzer
{
    /// <summary>
    /// 检查 Mods 目录下是否有 XNB 替换文件，以及 Content/Data 中的 XNB 引用冲突。
    /// 当前为占位实现，v3.1 将使用嵌入的 XNB 解析器。
    /// </summary>
    public static void Analyze(string gameRootPath, List<ModResult> mods)
    {
        // XNB 解析需要 MonoGame.Content.Builder 或嵌入式 pyxnb
        // v3.0 MVP: 跳过 XNB 级检测，仅标记为未实现
        foreach (var mod in mods)
        {
            string modDir = FindModDirectory(mod.FolderName);
            if (modDir == null) continue;

            // 检查是否有 .xnb 文件直接放在模组中（旧的 XNB 替换方式）
            var xnbFiles = Directory.EnumerateFiles(modDir, "*.xnb", SearchOption.AllDirectories).ToList();
            if (xnbFiles.Count > 0)
            {
                mod.Issues.Add(new Issue
                {
                    ModName = mod.DisplayName ?? mod.FolderName,
                    Category = "xnb_replacement",
                    Severity = Severity.Info,
                    Title = $"包含 {xnbFiles.Count} 个 XNB 替换文件",
                    Explanation = "此模组使用 XNB 文件直接替换游戏内容。建议迁移到 Content Patcher。",
                    FixSuggestion = "考虑使用 Content Patcher 的 CP 格式替代 XNB 替换。"
                });
            }
        }
    }

    private static string? FindModDirectory(string folderName)
    {
        if (Path.IsPathRooted(folderName) && Directory.Exists(folderName))
            return folderName;

        string cwd = Directory.GetCurrentDirectory();
        foreach (string dir in Directory.EnumerateDirectories(cwd))
        {
            if (string.Equals(Path.GetFileName(dir), folderName, StringComparison.OrdinalIgnoreCase))
                return dir;
        }
        return null;
    }
}
