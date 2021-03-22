using System.Diagnostics;
using System.Runtime.CompilerServices;
using Foundation;
using Microsoft.Maui;
using UIKit;

namespace Maui.Controls.Sample
{
	public class CustomIosLifecycleHandler : IosApplicationDelegateHandler
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			LogMember();

			return true;
		}

		public override void OnActivated(UIApplication application) => LogMember();

		public override void OnResignActivation(UIApplication application) => LogMember();

		public override void WillTerminate(UIApplication application) => LogMember();

		static void LogMember([CallerMemberName] string name = "") =>
			Debug.WriteLine("LIFECYCLE: " + name);
	}
}