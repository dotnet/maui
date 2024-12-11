using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18831 : _IssuesUITest
	{
		public Issue18831(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Border Shadow in ListView not showing";

		[Test]
		[Category(UITestCategories.ListView)]
		public void BorderShadowShouldWorkInListView()
		{
			App.WaitForElement("Item1");
			VerifyScreenshot();
		}
	}
}