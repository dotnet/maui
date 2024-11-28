using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2653 : _IssuesUITest
	{
		const string ButtonText = "Insert Box View";
		const string TestForButtonClicked = "Test For Clicked";
		const string Success = "BoxView Not Overlapping";

		public Issue2653(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Grid insert z-order on UWP broken in Forms 3";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void ZIndexWhenInsertingChildren()
		{
			App.WaitForNoElement(ButtonText);
			App.Tap(ButtonText);
			App.Tap(ButtonText);
			App.Tap(TestForButtonClicked);
			App.WaitForNoElement(Success);
		}
	}
}