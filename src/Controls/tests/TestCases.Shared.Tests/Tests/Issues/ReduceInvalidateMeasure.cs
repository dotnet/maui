using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class ReduceInvalidateMeasure : _IssuesUITest
	{
		public ReduceInvalidateMeasure(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "https://github.com/dotnet/maui/pull/21801";

		[Test]
		[Category(UITestCategories.Performance)]
		public void ReduceInvalidateMeasuresUpdatingLabel()
		{
			App.WaitForElement("UpdateTextLabel");

			const int repeats = 2;

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateTextButton");
			}

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateSizeButton");
			}

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateFontSizeButton");
			}

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateLineBreakModeButton");
			}

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateLineHeightButton");
			}

			for (int i = 0; i < repeats; i++)
			{
				App.Tap("UpdateVisibilityButton");
			}

			VerifyScreenshot();
		}
	}
}