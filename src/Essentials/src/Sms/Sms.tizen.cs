using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.Applications;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported
			=> PlatformUtils.GetFeatureInfo<bool>("network.telephony.sms");

		Task PlatformComposeAsync(SmsMessage message)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.Compose,
				Uri = "sms:",
			};

			if (!string.IsNullOrEmpty(message.Body))
				appControl.ExtraData.Add(AppControlData.Text, message.Body);
			if (message.Recipients.Count > 0)
				appControl.Uri += string.Join(" ", message.Recipients);

			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}
	}
}
