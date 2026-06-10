using Newtonsoft.Json;

namespace ModChecker.Models;

/// <summary>content.json 中的单条 Patch 或条目结构</summary>
public class ContentEntry
{
    [JsonProperty("Action")]
    public string? Action { get; set; }

    [JsonProperty("Target")]
    public string? Target { get; set; }

    [JsonProperty("FromFile")]
    public string? FromFile { get; set; }

    [JsonProperty("TargetLocale")]
    public string? TargetLocale { get; set; }

    [JsonProperty("When")]
    public Dictionary<string, string>? When { get; set; }
}

/// <summary>content.json 根结构</summary>
public class ContentConfig
{
    [JsonProperty("Format")]
    public string? Format { get; set; }

    [JsonProperty("Changes")]
    public List<ContentEntry>? Changes { get; set; }
}
