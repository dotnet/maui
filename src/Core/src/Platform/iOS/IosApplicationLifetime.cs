using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public class IosApplicationLifetime : IIosApplicationLifetime
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

		public virtual void DidEnterBackground(UIApplication application)
		{

		}

		public virtual void WillEnterForeground(UIApplication application)
		{

		}
	}
}