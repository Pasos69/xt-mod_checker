using System.Reflection;
using ModChecker.Models;

namespace ModChecker.Scanner;

/// <summary>
/// SMAPI.Toolkit 加载器。
/// 反射调用真实的 SMAPI.Toolkit 解析 manifest，
/// 不可用时回退到 Newtonsoft.Json。
/// </summary>
public static class SmapiToolkitLoader
{
    private static Assembly? _toolkitAsm;
    private static object? _jsonHelper;
    private static MethodInfo? _deserializeMethod;
    private static bool _initialized;

    /// <summary>尝试从游戏目录加载 SMAPI.Toolkit</summary>
    public static bool TryInitialize(string? gameRoot)
    {
        if (_initialized) return _toolkitAsm != null;
        _initialized = true;

        if (string.IsNullOrEmpty(gameRoot)) return false;

        string dllPath = Path.Combine(gameRoot, "smapi-internal", "SMAPI.Toolkit.dll");
        if (!File.Exists(dllPath)) return false;

        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDep;

            _toolkitAsm = Assembly.LoadFrom(dllPath);

            // SMAPI.Toolkit 使用 JsonHelper 作为序列化入口
            var helperType = _toolkitAsm.GetType(
                "StardewModdingAPI.Toolkit.Serialization.JsonHelper");
            if (helperType == null) { _toolkitAsm = null; return false; }

            _jsonHelper = Activator.CreateInstance(helperType);
            if (_jsonHelper == null) { _toolkitAsm = null; return false; }

            // Deserialize<T>(string json) — 这是 SMAPI 解析 manifest 的方法
            _deserializeMethod = helperType.GetMethods()
                .FirstOrDefault(m => m.Name == "Deserialize"
                                  && m.IsGenericMethod
                                  && m.GetParameters().Length == 1
                                  && m.GetParameters()[0].ParameterType == typeof(string));
            if (_deserializeMethod == null) { _toolkitAsm = null; return false; }

            Console.WriteLine("  [SMAPI.Toolkit]  已加载，使用真实 SMAPI 解析引擎");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [SMAPI.Toolkit] 加载失败（将回退到内置解析器）: {ex.Message}");
            _toolkitAsm = null;
            return false;
        }
    }

    /// <summary>使用 SMAPI.Toolkit 解析 manifest.json</summary>
    public static Manifest? DeserializeManifest(string json)
    {
        if (_toolkitAsm == null || _deserializeMethod == null || _jsonHelper == null)
            return null;

        try
        {
            // 获取 SMAPI 的 Manifest 类型
            var smapiManifestType = _toolkitAsm.GetType(
                "StardewModdingAPI.Toolkit.Serialization.Models.Manifest");

            // Deserialize<Manifest>(json)
            var genericMethod = _deserializeMethod.MakeGenericMethod(smapiManifestType!);
            var result = genericMethod.Invoke(_jsonHelper, new object[] { json });
            if (result == null) return null;

            return MapToManifest(result);
        }
        catch { return null; }
    }

    private static Manifest? MapToManifest(object smapiManifest)
    {
        try
        {
            var manifest = new Manifest
            {
                Name = GetStr(smapiManifest, "Name"),
                UniqueID = GetStr(smapiManifest, "UniqueID"),
                Version = GetStr(smapiManifest, "Version"),
                MinimumApiVersion = GetStr(smapiManifest, "MinimumApiVersion"),
                EntryDll = GetStr(smapiManifest, "EntryDll")
            };

            // ContentPackFor
            var cpk = GetProp(smapiManifest, "ContentPackFor");
            if (cpk != null)
            {
                manifest.ContentPackFor = new ManifestContentPackFor
                {
                    UniqueID = GetStr(cpk, "UniqueID"),
                    MinimumVersion = GetStr(cpk, "MinimumVersion")
                };
            }

            // UpdateKeys (string[])
            var keys = GetProp(smapiManifest, "UpdateKeys");
            if (keys is string[] strArray)
                manifest.UpdateKeys = new List<string>(strArray);
            else if (keys is System.Collections.IEnumerable e)
            {
                manifest.UpdateKeys = new List<string>();
                foreach (var item in e)
                    manifest.UpdateKeys.Add(item?.ToString() ?? "");
            }

            // Dependencies
            var deps = GetProp(smapiManifest, "Dependencies");
            if (deps is System.Collections.IEnumerable depEnum)
            {
                manifest.Dependencies = new List<ManifestDependency>();
                foreach (var d in depEnum)
                {
                    manifest.Dependencies.Add(new ManifestDependency
                    {
                        UniqueID = GetStr(d, "UniqueID"),
                        MinimumVersion = GetStr(d, "MinimumVersion"),
                        IsRequired = GetBool(d, "IsRequired", true)
                    });
                }
            }

            return manifest;
        }
        catch { return null; }
    }

    private static object? GetProp(object obj, string n)
        => obj.GetType().GetProperty(n)?.GetValue(obj);

    private static string GetStr(object obj, string n)
        => GetProp(obj, n)?.ToString() ?? "";

    private static bool GetBool(object obj, string n, bool def)
        => GetProp(obj, n) is bool b ? b : def;

    private static Assembly? ResolveDep(object? s, ResolveEventArgs args)
    {
        string name = new AssemblyName(args.Name).Name;
        if (name is "netstandard" or "System.Runtime" or "System.Collections")
            return null;

        string gameRoot = Path.GetDirectoryName(
            Path.GetDirectoryName(Directory.GetCurrentDirectory())) ?? "";
        string dllPath = Path.Combine(gameRoot, "smapi-internal", $"{name}.dll");
        return File.Exists(dllPath) ? Assembly.LoadFrom(dllPath) : null;
    }
}
