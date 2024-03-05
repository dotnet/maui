using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1601 : IssuesUITest
	{
		public Issue1601(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception thrown when `Removing Content Using LayoutCompression"; 
		
		[Test]
		[Category(UITestCategories.Layout)]
		public void Issue1601Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.Screenshot("Start G1601");
			App.WaitForElement("CrashButton");
			App.Screenshot("I see the button");
			App.Click("CrashButton");
			App.Screenshot("Didn't crash!");
		}
	}
}