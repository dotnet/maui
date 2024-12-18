using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla58645 : _IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla58645(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] NRE Thrown When ListView Items Are Replaced By Items With a Different Template";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla58645Test()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
		}
	}
}