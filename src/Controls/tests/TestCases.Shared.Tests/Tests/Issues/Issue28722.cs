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
			NavigateBack();
			App.WaitForElement("HelloLabel");

			NavigateBack();
			App.WaitForElement("HelloLabel");
		}

		// On Windows, BackButtonBehavior.TextOverride is not supported (https://github.com/dotnet/maui/issues/1625),
		// so the "Click" element does not exist. Use TapBackArrow to navigate back instead.
		void NavigateBack()
		{
#if WINDOWS
			App.TapBackArrow();
#elif ANDROID
			App.TapBackArrow("Click");
#else
			App.WaitForElement("Click");
			App.Click("Click");
#endif
		}
	}
}