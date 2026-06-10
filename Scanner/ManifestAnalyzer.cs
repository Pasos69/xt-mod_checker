using ModChecker.Models;

namespace ModChecker.Scanner;

/// <summary>manifest.json 深度分析</summary>
public static class ManifestAnalyzer
{
    private static readonly HashSet<string> KnownUniqueIds = new();
    private static readonly HashSet<string> FrameworkModIds = new()
    {
        "Pathoschild.ContentPatcher",
        "spacechase0.SpaceCore",
        "Platonymous.Toolkit",
        "Esca.FarmTypeManager",
        "spacechase0.JsonAssets",
        "spacechase0.ExtraMapLayers",
        "ceruleandeep.CustomEmojis",
        "aedenthorn.CustomMusic",
        "DIGUS.MailFrameworkMod",
        "BFAVB.CustomNPCFixes"
    };

    public static void Analyze(ModResult mod, Manifest? manifest, List<ModResult> allMods)
    {
        if (manifest == null) return;

        // UniqueID 唯一性
        if (manifest.UniqueID != null)
        {
            if (KnownUniqueIds.Contains(manifest.UniqueID))
            {
                mod.Issues.Add(new Issue
                {
                    ModName = mod.DisplayName ?? mod.FolderName,
                    Category = "duplicate_unique_id",
                    Severity = Severity.Error,
                    Title = $"UniqueID 重复: {manifest.UniqueID}",
                    Explanation = "多个模组使用了相同的 UniqueID，SMAPI 只会加载其中一个。",
                    FixSuggestion = "修改此模组的 UniqueID 确保全局唯一。"
                });
            }
            KnownUniqueIds.Add(manifest.UniqueID);
        }

        // 必填字段
        if (string.IsNullOrWhiteSpace(manifest.Name))
            AddMissingField(mod, "Name");
        if (string.IsNullOrWhiteSpace(manifest.UniqueID))
            AddMissingField(mod, "UniqueID");
        if (string.IsNullOrWhiteSpace(manifest.Version))
            AddMissingField(mod, "Version");

        // EntryDll 存在性
        if (manifest.EntryDll != null)
        {
            string dllPath = Path.Combine(
                Path.GetDirectoryName(mod.FolderName) ?? ".",
                manifest.EntryDll);
            if (!File.Exists(dllPath))
            {
                mod.Issues.Add(new Issue
                {
                    ModName = mod.DisplayName ?? mod.FolderName,
                    Category = "missing_entry_dll",
                    Severity = Severity.Error,
                    Title = $"EntryDll 文件不存在: {manifest.EntryDll}",
                    Explanation = "manifest.json 指定的 EntryDll 文件在模组文件夹中未找到。",
                    FixSuggestion = "确认 DLL 文件名拼写正确，或重新安装模组。",
                    CodeSnippet = manifest.EntryDll
                });
            }
        }

        // UpdateKeys 格式
        if (manifest.UpdateKeys != null)
        {
            foreach (string key in manifest.UpdateKeys)
            {
                if (!key.Contains(':') && !key.StartsWith("Nexus:", StringComparison.OrdinalIgnoreCase))
                {
                    mod.Issues.Add(new Issue
                    {
                        ModName = mod.DisplayName ?? mod.FolderName,
                        Category = "invalid_update_key",
                        Severity = Severity.Warning,
                        Title = $"UpdateKey 格式异常: {key}",
                        Explanation = "UpdateKeys 格式通常为 \"Nexus:ID\" 或 \"GitHub:owner/repo\"。",
                        FixSuggestion = "检查填写格式是否正确。",
                        CodeSnippet = key
                    });
                }
            }
        }

        // ContentPackFor 引用的模组是否存在
        if (manifest.ContentPackFor?.UniqueID != null)
        {
            string targetId = manifest.ContentPackFor.UniqueID;
            var target = allMods.Find(m =>
                string.Equals(m.UniqueID, targetId, StringComparison.OrdinalIgnoreCase));

            if (target == null && !FrameworkModIds.Contains(targetId))
            {
                mod.Issues.Add(new Issue
                {
                    ModName = mod.DisplayName ?? mod.FolderName,
                    Category = "missing_content_pack_for",
                    Severity = Severity.Error,
                    Title = $"ContentPackFor 目标不存在: {targetId}",
                    Explanation = "此内容包依赖的框架模组未安装。",
                    FixSuggestion = $"安装 \"{targetId}\" 对应的模组。"
                });
            }
        }

        // MinimumApiVersion 检查
        if (manifest.MinimumApiVersion != null && SmapiVersion.TryParse(manifest.MinimumApiVersion, out var minVer))
        {
            mod.Issues.Add(new Issue
            {
                ModName = mod.DisplayName ?? mod.FolderName,
                Category = "minimum_api_version",
                Severity = Severity.Info,
                Title = $"最低 SMAPI 版本: {manifest.MinimumApiVersion}",
                Explanation = "此模组要求的最小 SMAPI 版本。当前版本将在此处显示并比对。",
                CodeSnippet = manifest.MinimumApiVersion
            });
        }
    }

    private static void AddMissingField(ModResult mod, string field)
    {
        mod.Issues.Add(new Issue
        {
            ModName = mod.DisplayName ?? mod.FolderName,
            Category = "missing_field",
            Severity = Severity.Error,
            Title = $"缺少必填字段: {field}",
            Explanation = $"manifest.json 中缺少 \"{field}\" 字段，SMAPI 可能无法正常加载此模组。",
            FixSuggestion = $"在 manifest.json 中添加 \"{field}\" 字段。"
        });
    }

    /// <summary>获取已知的框架模组 ID 列表</summary>
    public static HashSet<string> GetFrameworkModIds() => FrameworkModIds;
}
