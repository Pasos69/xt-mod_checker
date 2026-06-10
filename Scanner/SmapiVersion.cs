using System.Text.RegularExpressions;

namespace ModChecker.Scanner;

/// <summary>SMAPI 版本号解析（支持 x.y.z 格式）</summary>
public partial class SmapiVersion : IComparable<SmapiVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    private SmapiVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    [GeneratedRegex(@"^(\d+)\.(\d+)\.?(\d+)?")]
    private static partial Regex VersionPattern();

    public static bool TryParse(string? input, out SmapiVersion? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var m = VersionPattern().Match(input);
        if (!m.Success) return false;

        int major = int.Parse(m.Groups[1].Value);
        int minor = int.Parse(m.Groups[2].Value);
        int patch = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
        version = new SmapiVersion(major, minor, patch);
        return true;
    }

    public int CompareTo(SmapiVersion? other)
    {
        if (other == null) return 1;
        int cmp = Major.CompareTo(other.Major);
        if (cmp != 0) return cmp;
        cmp = Minor.CompareTo(other.Minor);
        if (cmp != 0) return cmp;
        return Patch.CompareTo(other.Patch);
    }

    public static bool operator >=(SmapiVersion a, SmapiVersion b) => a.CompareTo(b) >= 0;
    public static bool operator <=(SmapiVersion a, SmapiVersion b) => a.CompareTo(b) <= 0;

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
