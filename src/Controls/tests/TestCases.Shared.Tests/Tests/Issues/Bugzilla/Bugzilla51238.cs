using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla51238 : _IssuesUITest
	{
		public Bugzilla51238(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Transparent Grid causes Java.Lang.IllegalStateException: Unable to create layer for Platform_DefaultRenderer";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Issue1Test()
		{
			App.WaitForElement("TapMe");
			App.Tap("TapMe"); // Crashes the app if the issue isn't fixed
			App.WaitForElement("TapMe");
		}
	}
}