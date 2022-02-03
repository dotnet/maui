using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported
			=> Platform.GetFeatureInfo<bool>("contact");

		public void PlatformOpen(string number)
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
