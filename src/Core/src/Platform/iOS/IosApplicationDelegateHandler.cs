using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public class IosApplicationDelegateHandler : IIosApplicationDelegateHandler
	{
		public virtual bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			return true;
		}

		public virtual void OnActivated(UIApplication application)
		{

		}

		public virtual void OnResignActivation(UIApplication application)
		{

		}

		public virtual void WillTerminate(UIApplication application)
		{

		}
	}
}