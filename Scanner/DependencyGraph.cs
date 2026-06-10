using ModChecker.Models;
using Newtonsoft.Json;

namespace ModChecker.Scanner;

/// <summary>依赖图构建与缺失依赖检测</summary>
public static class DependencyGraph
{
    public static void Analyze(List<ModResult> allMods, string modsPath)
    {
        var uniqueIdMap = allMods
            .Where(m => m.UniqueID != null)
            .ToDictionary(m => m.UniqueID!, m => m, StringComparer.OrdinalIgnoreCase);

        foreach (var mod in allMods)
        {
            if (!mod.HasManifest) continue;

            string manifestPath = Path.Combine(modsPath, mod.FolderName, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            try
            {
                string json = File.ReadAllText(manifestPath);
                var manifest = SmapiToolkitLoader.DeserializeManifest(json)
                               ?? JsonConvert.DeserializeObject<Models.Manifest>(json);
                if (manifest?.Dependencies == null) continue;

                foreach (var dep in manifest.Dependencies)
                {
                    if (dep.UniqueID == null) continue;

                    bool exists = uniqueIdMap.ContainsKey(dep.UniqueID) ||
                                  ManifestAnalyzer.GetFrameworkModIds().Contains(dep.UniqueID);

                    if (!exists && dep.IsRequired)
                    {
                        mod.Issues.Add(new Issue
                        {
                            ModName = mod.DisplayName ?? mod.FolderName,
                            Category = "missing_dependency",
                            Severity = Severity.Error,
                            Title = $"缺少依赖: {dep.UniqueID}",
                            Explanation = $"此模组需要 \"{dep.UniqueID}\" 但未安装。",
                            FixSuggestion = $"安装 \"{dep.UniqueID}\" 模组。",
                            CodeSnippet = dep.UniqueID
                        });
                    }
                }
            }
            catch { /* 跳过无法解析的 manifest */ }
        }
    }
}
