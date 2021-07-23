using System.Windows.Input;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Utilities
{
	static class Commands
	{
		static Commands()
		{
			LaunchUrl = new DelegateCommand<string>(OnLaunchUrl);
		}

		public static ICommand LaunchUrl { get; private set; }

		static void OnLaunchUrl(string str)
		{
			Launcher.TryOpenAsync(str);
		}
	}
}