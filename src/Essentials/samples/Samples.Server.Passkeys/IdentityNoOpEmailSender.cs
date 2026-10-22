using Microsoft.AspNetCore.Identity;

namespace Samples.Server.Passkeys;

// Dev-only test server: Identity's registration flow requires an IEmailSender<TUser> to be
// registered, but this headless sample never sends email (email confirmation is disabled in
// Program.cs). Every method is a no-op. Do not copy this into production.
internal sealed class IdentityNoOpEmailSender : IEmailSender<IdentityUser>
{
	public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink) =>
		Task.CompletedTask;

	public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) =>
		Task.CompletedTask;

	public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode) =>
		Task.CompletedTask;
}
