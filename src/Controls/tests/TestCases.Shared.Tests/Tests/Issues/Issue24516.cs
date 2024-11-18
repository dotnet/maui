#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24516 : _IssuesUITest
	{
		public Issue24516(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Rendering issue in WinUI when setting Label.FormattedText";

		[Test]
		[Category(UITestCategories.Label)]
		public void VerifyLabelTextColorWhenResettingFromFormattedText()
		{
			App.WaitForElement("Label");
			App.Tap("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
#endif