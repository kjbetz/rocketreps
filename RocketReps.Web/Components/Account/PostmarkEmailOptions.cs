namespace RocketReps.Web.Components.Account;

internal sealed class PostmarkEmailOptions
{
    public const string SectionName = "Postmark";

    public string? ServerToken { get; set; }

    public string? FromEmail { get; set; }

    public string MessageStream { get; set; } = "outbound";
}
