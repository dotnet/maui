using UIKit;

namespace Microsoft.Maui.TestUtils
{
	public class BaseTestViewController : UIViewController
	{
		readonly BaseTestApplicationDelegate _applicationDelegate;

		public BaseTestViewController(BaseTestApplicationDelegate applicationDelegate)
		{
			_applicationDelegate = applicationDelegate;
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

			var entryPoint = new TestEntryPoint(_applicationDelegate);

			await entryPoint.RunAsync();
		}
	}
}