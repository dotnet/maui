using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla51505 : _IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla51505(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObjectDisposedException On Effect detachment.";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Effects)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Bugzilla51505Test()
		{
			App.WaitForElement(ButtonId);
			Assert.DoesNotThrow(() => App.Tap(ButtonId));
		}
	}
}