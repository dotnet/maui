using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.Applications;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported
			=> PlatformUtils.GetFeatureInfo<bool>("email");

		Task PlatformComposeAsync(EmailMessage message)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.Compose,
				Uri = "mailto:",
			};

			if (message.Bcc?.Count > 0)
				appControl.ExtraData.Add(AppControlData.Bcc, message.Bcc);
			if (!string.IsNullOrEmpty(message.Body))
				appControl.ExtraData.Add(AppControlData.Text, message.Body);
			if (message.Cc?.Count > 0)
				appControl.ExtraData.Add(AppControlData.Cc, message.Cc);
			if (!string.IsNullOrEmpty(message.Subject))
				appControl.ExtraData.Add(AppControlData.Subject, message.Subject);
			if (message.To?.Count > 0)
				appControl.ExtraData.Add(AppControlData.To, message.To);

			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}

		public Task ComposeAsync(string subject, string body, params string[] to)
			=> ComposeAsync(
				new EmailMessage
				{
					Subject = subject,
					Body = body,
					To = to.ToList()
				});

		public Task ComposeAsync()
			=> ComposeAsync(null);
	}
}
