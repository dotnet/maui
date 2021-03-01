using Microsoft.Maui.TestUtils;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public class Application
	{
		static void Main(string[] args)
		{
			if (BaseTestApplicationDelegate.IsXHarnessRun(args))
				UIApplication.Main(args, null, nameof(TestApplicationDelegate));
			else
				UIApplication.Main(args, null, nameof(AppDelegate));
		}
	}
}