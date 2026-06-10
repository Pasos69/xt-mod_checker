using ModChecker.Models;
using Newtonsoft.Json;

namespace ModChecker.Scanner;

/// <summary>content.json 分析：JSON 语法、缺失文件、Patch Target 重叠</summary>
public static class ContentAnalyzer
{
    public static void Analyze(ModResult mod, string modPath)
    {
        string contentPath = Path.Combine(modPath, "content.json");
        if (!File.Exists(contentPath)) return;

        // 解析 content.json
        ContentConfig? config;
        try
        {
            string json = File.ReadAllText(contentPath);
            config = JsonConvert.DeserializeObject<ContentConfig>(json);

            if (config?.Format != null)
            {
                mod.Issues.Add(new Issue
                {
                    ModName = mod.DisplayName ?? mod.FolderName,
                    Category = "content_format",
                    Severity = Severity.Info,
                    Title = $"Format: {config.Format}",
                    Explanation = "CP 格式版本号。",
                    CodeSnippet = config.Format
                });
            }
        }
        catch (JsonReaderException ex)
        {
            mod.Issues.Add(new Issue
            {
                ModName = mod.DisplayName ?? mod.FolderName,
                Category = "json_parse",
                Severity = Severity.Error,
                Title = $"content.json JSON 错误: {ex.Message}",
                Explanation = "content.json 存在 JSON 语法错误，CP 可能无法正确加载。",
                FixSuggestion = "使用 JSON 验证工具检查语法。",
                SourceFile = contentPath,
                CodeSnippet = $"Line: {ex.LineNumber}, Pos: {ex.LinePosition}"
            });
            return;
        }
        catch (Exception ex)
        {
            mod.Issues.Add(new Issue
            {
                ModName = mod.DisplayName ?? mod.FolderName,
                Category = "json_parse",
                Severity = Severity.Error,
                Title = $"content.json 读取失败: {ex.Message}"
            });
            return;
        }

        if (config?.Changes == null || config.Changes.Count == 0) return;

        string modDir = Path.GetDirectoryName(contentPath)!;
        foreach (var entry in config.Changes)
        {
            // 检查 FromFile 指向的文件是否存在
            if (entry.FromFile != null)
            {
                // 处理相对路径
                string resolvedPath = ResolveFromFile(entry.FromFile, modDir);
                if (!File.Exists(resolvedPath))
                {
                    mod.Issues.Add(new Issue
                    {
                        ModName = mod.DisplayName ?? mod.FolderName,
                        Category = "missing_file",
                        Severity = Severity.Error,
                        Title = $"缺失文件: {NormalizePath(entry.FromFile)}",
                        Explanation = "content.json 引用的文件在模组文件夹中不存在。",
                        FixSuggestion = "检查是否下载完整，或文件名拼写是否有误。",
                        SourceFile = contentPath,
                        CodeSnippet = entry.FromFile
                    });
                }
            }
        }
    }

    /// <summary>解析 FromFile 相对路径为实际路径</summary>
    private static string ResolveFromFile(string fromFile, string modDir)
    {
        // 如果是绝对路径或包含 ..
        string normalized = fromFile.Replace('/', Path.DirectorySeparatorChar);
        string combined = Path.Combine(modDir, normalized);
        return Path.GetFullPath(combined);
    }

    /// <summary>标准化路径显示</summary>
    private static string NormalizePath(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/');
}
