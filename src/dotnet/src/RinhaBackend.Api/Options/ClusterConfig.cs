namespace RinhaBackend.Api;

public record ClusterConfig
{
    public const string CONFIG_NAME = "ClusterConfig";
    public string? ClusterId { get; set; }
    public string? ServiceId { get; set; }
    public string? AdoNetInvariant { get; set; }
    public string? ConnectionString { get; set; }
}
