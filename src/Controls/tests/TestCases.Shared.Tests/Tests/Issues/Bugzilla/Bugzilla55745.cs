using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla55745 : _IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla55745(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] NRE in ListView with HasUnevenRows=true after changing content and rebinding";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla55745Test()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.Tap(ButtonId);
		}
	}
}