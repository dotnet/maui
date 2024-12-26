using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26494 : _IssuesUITest
	{
		public Issue26494(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView headers gets invisible when collapsing/expanding sections";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void HeadersShouldBeVisible()
		{
			App.WaitForElement("Group0");

			App.Click("Group0");
			App.Click("Group1");
			App.Click("Group2");

			App.WaitForElement("Group0");
			App.WaitForElement("Group1");
			App.WaitForElement("Group2");
		}
	}
}