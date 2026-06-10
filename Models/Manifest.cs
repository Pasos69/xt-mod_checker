using Newtonsoft.Json;

namespace ModChecker.Models;

/// <summary>SMAPI mod manifest.json 结构</summary>
public class Manifest
{
    [JsonProperty("Name")]
    public string? Name { get; set; }

    [JsonProperty("UniqueID")]
    public string? UniqueID { get; set; }

    [JsonProperty("Version")]
    public string? Version { get; set; }

    [JsonProperty("MinimumApiVersion")]
    public string? MinimumApiVersion { get; set; }

    [JsonProperty("EntryDll")]
    public string? EntryDll { get; set; }

    [JsonProperty("ContentPackFor")]
    public ManifestContentPackFor? ContentPackFor { get; set; }

    [JsonProperty("UpdateKeys")]
    public List<string>? UpdateKeys { get; set; }

    [JsonProperty("Dependencies")]
    public List<ManifestDependency>? Dependencies { get; set; }
}

public class ManifestContentPackFor
{
    [JsonProperty("UniqueID")]
    public string? UniqueID { get; set; }

    [JsonProperty("MinimumVersion")]
    public string? MinimumVersion { get; set; }
}

public class ManifestDependency
{
    [JsonProperty("UniqueID")]
    public string? UniqueID { get; set; }

    [JsonProperty("MinimumVersion")]
    public string? MinimumVersion { get; set; }

    [JsonProperty("IsRequired")]
    public bool IsRequired { get; set; } = true;
}
