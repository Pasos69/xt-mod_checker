using System.Diagnostics;
using ModChecker.Models;
using ModChecker.Report;
using ModChecker.Scanner;

namespace ModChecker;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var sw = Stopwatch.StartNew();

        Console.WriteLine("SMAPI Mod Checker v3.0");
        Console.WriteLine("正在扫描模组...");
        Console.WriteLine();

        // 1. 路径解析
        string modsPath = PathResolver.ResolveModsPath(args);
        string? gameRoot = PathResolver.ResolveGameRoot(modsPath);

        Console.WriteLine($"模组目录: {modsPath}");
        Console.WriteLine($"游戏根目录: {gameRoot ?? "(未找到)"}");
        Console.WriteLine();

        // 2. 检测环境信息
        string smapiVersion = DetectSmapiVersion(gameRoot);
        string gameVersion = DetectGameVersion(gameRoot);
        Console.WriteLine($"SMAPI: {smapiVersion} | 星露谷: {gameVersion}");
        Console.WriteLine();

        // 2.5 尝试加载真实的 SMAPI.Toolkit.dll
        bool smapiToolkitLoaded = SmapiToolkitLoader.TryInitialize(gameRoot);
        Console.WriteLine();

        // 3. 枚举模组
        var mods = ModEnumerator.Enumerate(modsPath);

        // 4. 分析
        foreach (var mod in mods)
        {
            string modDir = FindModDirectory(mod.FolderName, modsPath);
            if (modDir == null) continue;

            // 读取 manifest 做深度分析
            string manifestPath = Path.Combine(modDir, "manifest.json");
            if (File.Exists(manifestPath))
            {
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    // 优先用 SMAPI.Toolkit 真实解析引擎，回退到 Newtonsoft.Json
                    var manifest = SmapiToolkitLoader.DeserializeManifest(json)
                                   ?? Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(json);
                    if (manifest != null)
                        ManifestAnalyzer.Analyze(mod, manifest, mods);
                }
                catch { /* ModEnumerator 已处理 JSON 解析错误 */ }
            }

            // 分析 content.json
            ContentAnalyzer.Analyze(mod, modDir);
        }

        // 5. 依赖图
        DependencyGraph.Analyze(mods, modsPath);

        // 6. XNB 检测
        XnbAnalyzer.Analyze(gameRoot ?? modsPath, mods);

        sw.Stop();

        // 7. 聚合结果
        var scanResult = new ScanResult
        {
            GameRootPath = gameRoot,
            ModsPath = modsPath,
            SmapiVersion = smapiVersion,
            GameVersion = gameVersion,
            ScanTime = DateTime.Now,
            ElapsedMs = sw.ElapsedMilliseconds,
            Mods = mods
        };

        // 8. 生成报告
        var view = ReportModel.Aggregate(scanResult);
        string html = HtmlRenderer.Render(view);

        // 9. 输出
        string outputPath = Path.Combine(modsPath, "_mod_check_report.html");
        File.WriteAllText(outputPath, html);

        Console.WriteLine($"检测完成！共 {mods.Count} 个模组，{scanResult.ErrorCount} 错误，{scanResult.WarningCount} 警告，耗时 {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"报告已生成: {outputPath}");
        Console.WriteLine();

        // 10. 在浏览器中打开
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            };
            Process.Start(psi);
            Console.WriteLine("已自动在浏览器中打开报告。");
        }
        catch
        {
            Console.WriteLine($"请手动打开: {outputPath}");
        }
    }

    private static string DetectSmapiVersion(string? gameRoot)
    {
        if (gameRoot == null) return "未知";

        // 尝试读取 SMAPI 内部 DLL 版本
        string[] searchPaths =
        [
            Path.Combine(gameRoot, "smapi-internal", "StardewModdingAPI.dll"),
            Path.Combine(gameRoot, "StardewModdingAPI.exe"),
            Path.Combine(gameRoot, "SMAPI", "StardewModdingAPI.dll"),
        ];

        foreach (string path in searchPaths)
        {
            if (File.Exists(path))
            {
                try
                {
                    var version = FileVersionInfo.GetVersionInfo(path);
                    return version.FileVersion ?? version.ProductVersion ?? "未知";
                }
                catch { }
            }
        }

        return "未检测到 SMAPI";
    }

    private static string DetectGameVersion(string? gameRoot)
    {
        if (gameRoot == null) return "未知";

        string svPath = Path.Combine(gameRoot, "Stardew Valley.exe");
        if (File.Exists(svPath))
        {
            try
            {
                var version = FileVersionInfo.GetVersionInfo(svPath);
                return version.FileVersion ?? version.ProductVersion ?? "未知";
            }
            catch { }
        }

        return "未检测到";
    }

    private static string? FindModDirectory(string folderName, string modsPath)
    {
        string fullPath = Path.Combine(modsPath, folderName);
        if (Directory.Exists(fullPath))
            return fullPath;

        // 尝试模糊匹配
        foreach (string dir in Directory.EnumerateDirectories(modsPath))
        {
            if (string.Equals(Path.GetFileName(dir), folderName, StringComparison.OrdinalIgnoreCase))
                return dir;
        }
        return null;
    }
}

// rebuild
