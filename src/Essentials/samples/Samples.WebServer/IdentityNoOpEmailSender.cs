using Essentials.Samples.WebServer.Data;
using Microsoft.AspNetCore.Identity;

namespace Essentials.Samples.WebServer;

// Dev-only test server: Identity's registration flow requires an IEmailSender<TUser> to be
// registered, but this headless sample never sends email (email confirmation is disabled in
// Program.cs). Every method is a no-op. Do not copy this into production.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
		Task.CompletedTask;

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
		Task.CompletedTask;

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
		Task.CompletedTask;
}
