using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.CarouselView)]
	public class Issue22035 : _IssuesUITest
	{
		public Issue22035(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] CarouselView: VirtualView cannot be null here, when clearing and adding items on second navigation";

		[Test]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, App.Back() is not working as expected.")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue. More information: https://github.com/dotnet/maui/issues/22287")]
		public void CarouselViewVirtualViewNotNull()
		{
			for (int i = 0; i < 2; i++)
			{
				App.WaitForElement("TestNavigateButton");
				App.Tap("TestNavigateButton");
				App.WaitForElement("TestLoadButton");
				App.Tap("TestLoadButton");
				App.WaitForElement("Item1");
				App.Back();
			}

			// Without exceptions, the test has passed.
		}
	}
}
