using System.Diagnostics;
using Foundation;
using Microsoft.Maui;
using UIKit;

namespace Maui.Controls.Sample
{
	public class CustomIosLifecycleHandler : IosApplicationDelegateHandler
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Debug.WriteLine("CustomIosLifecycleHandler FinishedLaunching");

			return base.FinishedLaunching(application, launchOptions);
		}
	}
}
