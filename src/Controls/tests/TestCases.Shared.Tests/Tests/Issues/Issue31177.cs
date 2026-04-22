using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class Issue31177 : _IssuesUITest
	{
		public override string Issue => "[iOS] ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing";

		public Issue31177(TestDevice device) : base(device)
		{
		}

		[Test]
		[Description("ScrollToAsync called from OnAppearing should scroll to the target position after layout")]
		public void ScrollToAsyncFromOnAppearingWorks()
		{
			// The status label is updated to "Scrolled" after ScrollToAsync completes.
			// If the scroll worked, the SuccessLabel (placed below a 2000px spacer) should be visible.
			App.WaitForElement("StatusLabel");

			// Wait for OnAppearing scroll to complete
			App.WaitForElement("Scrolled");

			// The SuccessLabel is at the bottom of the scroll content (below a 2000px spacer).
			// It should be visible only if ScrollToAsync actually scrolled.
			App.WaitForElement("SuccessLabel");
		}
	}
}
