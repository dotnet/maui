using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28722 : _IssuesUITest
	{
		public Issue28722(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "IsEnabled does not work in BackButtonBehavior";

		[Test]
		[Category(UITestCategories.Shell)]
		public void IsEnabledShouldWorkInBackButtonBehavior()
		{
			// On Windows, BackButtonBehavior.TextOverride is not supported (https://github.com/dotnet/maui/issues/1625),
			// so the "Click" element does not exist. Use TapBackArrow to navigate back instead.
#if WINDOWS
			App.TapBackArrow();
#else
			App.WaitForElement("Click");
			App.Click("Click");
#endif
			App.WaitForElement("HelloLabel");
#if WINDOWS
			App.TapBackArrow();
#else
			App.Click("Click");
#endif
			App.WaitForElement("HelloLabel");
		}
	}
}