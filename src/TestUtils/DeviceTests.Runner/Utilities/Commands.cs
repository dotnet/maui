using System.Windows.Input;
using Microsoft.Maui.Essentials;

namespace Xunit.Runners.Utilities
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