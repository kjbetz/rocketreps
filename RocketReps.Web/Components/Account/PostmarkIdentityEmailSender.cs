using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PostmarkDotNet;
using RocketReps.Web.Data;

namespace RocketReps.Web.Components.Account;

internal sealed class PostmarkIdentityEmailSender(IOptions<PostmarkEmailOptions> options) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var actionUrl = WebUtility.HtmlDecode(confirmationLink);
        var htmlBody = BuildActionEmail(
            "Teacher launch sequence",
            "Confirm your email to start Rocket Reps",
            $"Hi {GetDisplayName(user)}, your teacher account is almost ready. Confirm your email and you can start creating classrooms, student logins, and practice missions.",
            "Confirm account",
            actionUrl,
            "If you did not create a Rocket Reps account, you can safely ignore this email.");
        var textBody = BuildActionText(
            "Confirm your email to start Rocket Reps",
            $"Hi {GetDisplayName(user)}, your teacher account is almost ready. Confirm your email and you can start creating classrooms, student logins, and practice missions.",
            "Confirm account",
            actionUrl,
            "If you did not create a Rocket Reps account, you can safely ignore this email.");

        return SendEmailAsync(
            email,
            "Confirm your Rocket Reps account",
            htmlBody,
            textBody);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var actionUrl = WebUtility.HtmlDecode(resetLink);
        var htmlBody = BuildActionEmail(
            "Password reset",
            "Reset your Rocket Reps password",
            $"Hi {GetDisplayName(user)}, use this secure link to choose a new password and get back to Mission Control.",
            "Reset password",
            actionUrl,
            "If you did not request a password reset, you can safely ignore this email.");
        var textBody = BuildActionText(
            "Reset your Rocket Reps password",
            $"Hi {GetDisplayName(user)}, use this secure link to choose a new password and get back to Mission Control.",
            "Reset password",
            actionUrl,
            "If you did not request a password reset, you can safely ignore this email.");

        return SendEmailAsync(
            email,
            "Reset your Rocket Reps password",
            htmlBody,
            textBody);
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var encodedCode = WebUtility.HtmlEncode(resetCode);
        var htmlBody = BuildCodeEmail(
            "Password reset",
            "Use this reset code",
            $"Hi {GetDisplayName(user)}, enter this code in Rocket Reps to reset your password.",
            encodedCode,
            "If you did not request a password reset, you can safely ignore this email.");
        var textBody = $"""
            Rocket Reps

            Use this reset code

            Hi {GetDisplayName(user)}, enter this code in Rocket Reps to reset your password.

            Reset code: {resetCode}

            If you did not request a password reset, you can safely ignore this email.
            """;

        return SendEmailAsync(
            email,
            "Reset your Rocket Reps password",
            htmlBody,
            textBody);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody, string textBody)
    {
        var postmarkOptions = options.Value;

        if (string.IsNullOrWhiteSpace(postmarkOptions.ServerToken))
        {
            throw new InvalidOperationException("Postmark:ServerToken must be configured before sending email.");
        }

        if (string.IsNullOrWhiteSpace(postmarkOptions.FromEmail))
        {
            throw new InvalidOperationException("Postmark:FromEmail must be configured before sending email.");
        }

        var message = new PostmarkMessage
        {
            From = postmarkOptions.FromEmail,
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            MessageStream = postmarkOptions.MessageStream,
        };

        var client = new PostmarkClient(postmarkOptions.ServerToken);
        var result = await client.SendMessageAsync(message);

        if (result.ErrorCode != 0)
        {
            throw new InvalidOperationException($"Postmark rejected the email: {result.Message}");
        }
    }

    private static string GetDisplayName(ApplicationUser user) =>
        string.IsNullOrWhiteSpace(user.DisplayName) ? "there" : user.DisplayName.Trim();

    private static string BuildActionText(string title, string message, string actionText, string actionUrl, string footer) =>
        $"""
        Rocket Reps

        {title}

        {message}

        {actionText}: {actionUrl}

        {footer}
        """;

    private static string BuildActionEmail(string eyebrow, string title, string message, string actionText, string actionUrl, string footer)
    {
        var encodedEyebrow = WebUtility.HtmlEncode(eyebrow);
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedMessage = WebUtility.HtmlEncode(message);
        var encodedActionText = WebUtility.HtmlEncode(actionText);
        var encodedActionUrl = WebUtility.HtmlEncode(actionUrl);
        var encodedFooter = WebUtility.HtmlEncode(footer);

        return BuildEmailShell(encodedEyebrow, encodedTitle, $"""
            <p style="margin:0 0 26px;color:#475467;font-size:16px;line-height:1.65;">{encodedMessage}</p>
            <table role="presentation" cellspacing="0" cellpadding="0" style="margin:0 0 28px;">
                <tr>
                    <td style="border-radius:999px;background:linear-gradient(135deg,#7c3aed,#22d3ee);box-shadow:0 14px 30px rgba(91,95,246,0.28);">
                        <a href="{encodedActionUrl}" style="display:inline-block;padding:14px 24px;color:#ffffff;font-size:15px;font-weight:800;letter-spacing:.01em;text-decoration:none;border-radius:999px;">{encodedActionText}</a>
                    </td>
                </tr>
            </table>
            <p style="margin:0 0 8px;color:#64748b;font-size:13px;line-height:1.6;">Button not working? Paste this link into your browser:</p>
            <p style="margin:0;word-break:break-all;color:#3838b8;font-size:13px;line-height:1.6;"><a href="{encodedActionUrl}" style="color:#3838b8;">{encodedActionUrl}</a></p>
            <p style="margin:26px 0 0;color:#64748b;font-size:13px;line-height:1.6;">{encodedFooter}</p>
            """);
    }

    private static string BuildCodeEmail(string eyebrow, string title, string message, string code, string footer)
    {
        var encodedEyebrow = WebUtility.HtmlEncode(eyebrow);
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedMessage = WebUtility.HtmlEncode(message);
        var encodedFooter = WebUtility.HtmlEncode(footer);

        return BuildEmailShell(encodedEyebrow, encodedTitle, $"""
            <p style="margin:0 0 24px;color:#475467;font-size:16px;line-height:1.65;">{encodedMessage}</p>
            <div style="margin:0 0 26px;padding:22px;border:1px solid rgba(91,95,246,0.22);border-radius:20px;background:#f6f9ff;text-align:center;">
                <p style="margin:0 0 8px;color:#64748b;font-size:12px;font-weight:800;letter-spacing:.16em;text-transform:uppercase;">Reset code</p>
                <p style="margin:0;color:#101828;font-size:32px;line-height:1.2;font-weight:900;letter-spacing:.18em;font-family:'SFMono-Regular',Consolas,'Liberation Mono',monospace;">{code}</p>
            </div>
            <p style="margin:0;color:#64748b;font-size:13px;line-height:1.6;">{encodedFooter}</p>
            """);
    }

    private static string BuildEmailShell(string eyebrow, string title, string content) =>
        $"""
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <title>{title}</title>
        </head>
        <body style="margin:0;padding:0;background:#eef4ff;font-family:Inter,-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif;color:#101828;">
            <div style="display:none;max-height:0;overflow:hidden;color:transparent;opacity:0;">{title}</div>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#eef4ff;background-image:radial-gradient(circle at 12% 0,rgba(91,95,246,.18),transparent 260px),radial-gradient(circle at 86% 8%,rgba(34,211,238,.22),transparent 260px);">
                <tr>
                    <td align="center" style="padding:32px 16px;">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:620px;overflow:hidden;border-radius:28px;background:#ffffff;box-shadow:0 24px 70px rgba(32,54,104,.15);">
                            <tr>
                                <td style="padding:0;background:linear-gradient(135deg,#131b3a,#4f46e5 58%,#22d3ee);">
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                        <tr>
                                            <td style="padding:30px 30px 26px;">
                                                <div style="display:inline-block;margin:0 0 22px;padding:9px 13px;border-radius:999px;background:rgba(255,255,255,.14);color:#ffffff;font-size:13px;font-weight:800;letter-spacing:.02em;">Rocket Reps</div>
                                                <p style="margin:0 0 8px;color:#a5f3fc;font-size:12px;font-weight:900;letter-spacing:.16em;text-transform:uppercase;">{eyebrow}</p>
                                                <h1 style="margin:0;color:#ffffff;font-size:32px;line-height:1.12;font-weight:900;letter-spacing:-.04em;">{title}</h1>
                                            </td>
                                            <td width="132" valign="bottom" style="padding:0 24px 20px 0;text-align:right;">
                                                <div style="display:inline-block;width:86px;height:86px;border-radius:28px;background:rgba(255,255,255,.16);text-align:center;color:#ffffff;font-size:42px;line-height:86px;font-weight:900;box-shadow:inset 0 0 0 1px rgba(255,255,255,.18);">R</div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:34px 30px 30px;">
                                    {content}
                                </td>
                            </tr>
                            <tr>
                                <td style="padding:22px 30px;background:#f8fbff;border-top:1px solid rgba(15,23,42,.08);">
                                    <p style="margin:0;color:#64748b;font-size:12px;line-height:1.6;">Rocket Reps helps teachers launch short, steady spaced-repetition practice for young learners.</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
}
