using Tizen.Applications;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported
			=> Platform.GetFeatureInfo<bool>("contact");

		public void Open(string number)
		{
			ValidateOpen(number);

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.Dial,
				Uri = "tel:",
			};

			if (!string.IsNullOrEmpty(number))
				appControl.Uri += number;

			AppControl.SendLaunchRequest(appControl);
		}
	}
}
