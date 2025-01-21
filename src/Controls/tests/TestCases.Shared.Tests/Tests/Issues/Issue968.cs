using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue968 : _IssuesUITest
	{
		public Issue968(TestDevice testDevice) : base(testDevice)
		{
		}
		const string StackLabel = "You should see me after rotating";
		public override string Issue => "StackLayout does not relayout on device rotation";

		[Test]
		[Description("Verify the layout lays out on rotations")]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMacWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
		public void Issue968TestsRotationRelayoutIssue()
		{
			App.WaitForElement("TestReady");
			App.SetOrientationLandscape();
			App.WaitForElement(StackLabel);
			App.SetOrientationPortrait();
			App.WaitForElement(StackLabel);
		}
	}
}