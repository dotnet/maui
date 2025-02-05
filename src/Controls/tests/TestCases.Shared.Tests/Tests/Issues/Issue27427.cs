using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27427 : _IssuesUITest
	{
		public override string Issue => "iOS SearchBar ignores WidthRequest and HeightRequest property values";

		public Issue27427(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void EnsureSearchBarExplicitSize()
		{
			App.WaitForElement("MainParentLayout");
			VerifyScreenshot();
		}
	}
}
