#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21980 : _IssuesUITest
	{
		public Issue21980(TestDevice device) : base(device)
		{
		}

		public override string Issue => "IndicatorView with DataTemplate (custom image) does not render correctly when ItemsSource change.";

		[Test]
		[Category(UITestCategories.IndicatorView)]
		public void IndicatorViewSizeAfterItemsSourceUpdate()
		{
			App.WaitForElement("changeItemsSource");
			App.Click("changeItemsSource");
			App.WaitForElement("changeItemsSource");
			VerifyScreenshot();
		}
	}
}
#endif