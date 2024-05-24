using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla57910 : _IssuesUITest
	{
		const string ButtonId = "btnPush";
		const string Button2Id = "btnPop";

		public Bugzilla57910(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		public void Bugzilla57910Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			for (int i = 0; i < 10; i++)
			{
				App.WaitForElement(ButtonId);
				App.Tap(ButtonId);
				App.WaitForElement(Button2Id);
				App.Tap(Button2Id);
			}
		}
		*/
	}
}