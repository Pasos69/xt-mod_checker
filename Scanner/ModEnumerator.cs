using ModChecker.Models;

namespace ModChecker.Scanner;

/// <summary>枚举 Mods 目录下的所有模组</summary>
public static class ModEnumerator
{
    public static List<ModResult> Enumerate(string modsPath)
    {
        var mods = new List<ModResult>();

        foreach (string dir in Directory.EnumerateDirectories(modsPath))
        {
            string dirName = Path.GetFileName(dir);
            string manifestPath = Path.Combine(dir, "manifest.json");
            string contentPath = Path.Combine(dir, "content.json");

            var mod = new ModResult
            {
                FolderName = dirName,
                HasManifest = File.Exists(manifestPath),
                HasContentJson = File.Exists(contentPath)
            };

            if (mod.HasManifest)
            {
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(json);
                    if (manifest != null)
                    {
                        mod.DisplayName = manifest.Name ?? dirName;
                        mod.UniqueID = manifest.UniqueID;
                        mod.Version = manifest.Version;
                        mod.MinimumApiVersion = manifest.MinimumApiVersion;
                        mod.ContentPackFor = manifest.ContentPackFor?.UniqueID;
                    }
                }
                catch
                {
                    mod.Issues.Add(new Issue
                    {
                        ModName = dirName,
                        Category = "json_parse",
                        Severity = Severity.Error,
                        Title = $"manifest.json 解析失败",
                        Explanation = "JSON 格式错误，无法读取模组信息。",
                        FixSuggestion = "使用 JSON 验证工具检查 manifest.json 语法。",
                        SourceFile = manifestPath
                    });
                }
            }
            else
            {
                mod.Issues.Add(new Issue
                {
                    ModName = dirName,
                    Category = "missing_manifest",
                    Severity = Severity.Error,
                    Title = "缺少 manifest.json",
                    Explanation = "此目录中没有 manifest.json 文件，SMAPI 不会加载它。",
                    FixSuggestion = "确认这是一个有效的模组目录，或将其移除。",
                    SourceFile = dir
                });
            }

            mods.Add(mod);
        }

        return mods;
    }
}
