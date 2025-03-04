using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26781 : _IssuesUITest
	{
		public Issue26781(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollToAsyncShouldWork()
		{
			App.WaitForElement("SuccessLabel");
		}
	}
}