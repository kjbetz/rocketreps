namespace RocketReps.Web.Data;

public sealed class RocketRepsDataProtectionOptions
{
    public const string SectionName = "DataProtection";

    public string ApplicationName { get; init; } = "RocketReps";

    public string? KeysDirectory { get; init; }
}
