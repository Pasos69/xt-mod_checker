namespace ModChecker.Scanner;

/// <summary>路径解析：支持拖拽和 Mods 目录内双击</summary>
public static class PathResolver
{
    /// <summary>从命令行参数或当前目录解析模组路径</summary>
    public static string ResolveModsPath(string[] args)
    {
        // 1. 拖拽场景: 第一个参数是目录路径
        if (args.Length > 0 && Directory.Exists(args[0]))
            return Path.GetFullPath(args[0]);

        // 2. 在 Mods 目录内双击: 检查当前目录
        string cwd = Directory.GetCurrentDirectory();

        if (IsModsDirectory(cwd))
            return cwd;

        // 3. 检查当前目录的父级是否有 Mods 目录
        string? parent = Directory.GetParent(cwd)?.FullName;
        if (parent != null)
        {
            string parentMods = Path.Combine(parent, "Mods");
            if (Directory.Exists(parentMods))
                return parentMods;
        }

        // 4. 都找不到 → 报错退出
        Console.Error.WriteLine("错误: 请将本程序放在 Mods 目录内运行，");
        Console.Error.WriteLine("       或拖拽一个 Mods 目录到本程序图标上。");
        Environment.Exit(1);
        return "";
    }

    /// <summary>从 Mods 路径向上查找游戏根目录</summary>
    public static string? ResolveGameRoot(string modsPath)
    {
        string? dir = Directory.GetParent(modsPath)?.FullName;
        while (dir != null)
        {
            string svExe = Path.Combine(dir, "Stardew Valley.exe");
            string smapiExe = Path.Combine(dir, "StardewModdingAPI.exe");
            if (File.Exists(svExe) || File.Exists(smapiExe))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    private static bool IsModsDirectory(string path)
    {
        string dirName = Path.GetFileName(path);
        if (!string.Equals(dirName, "Mods", StringComparison.OrdinalIgnoreCase))
            return false;

        // 检查是否包含至少一个 manifest.json
        return Directory.EnumerateFiles(path, "manifest.json", SearchOption.AllDirectories).Any();
    }
}
