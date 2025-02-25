using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue417 : _IssuesUITest
	{
		public Issue417(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation.PopToRootAsync does nothing";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Issue417TestsNavigateAndPopToRoot()
		{
			App.WaitForElement("First Page");
			App.WaitForElement("Next Page");

			App.Tap("Next Page");

			App.WaitForElement("Second Page");
			App.WaitForElement("Next Page 2");
			App.Tap("Next Page 2");

			App.WaitForElement("Third Page");
			App.WaitForElement("Pop to root");
			App.Tap("Pop to root");

			App.WaitForElement("First Page");
			App.WaitForElement("Next Page");
		}
	}
}