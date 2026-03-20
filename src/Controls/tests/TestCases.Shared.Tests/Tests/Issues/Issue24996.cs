using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24996 : _IssuesUITest
	{
		public Issue24996(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Changing Translation of an element causes Maui in iOS to constantly run Measure & ArrangeChildren";

		[Test]
		[Category(UITestCategories.Layout)]
		public void ChangingTranslationShouldNotCauseLayoutPassOnAncestors()
		{
			App.WaitForElement("Stats");
			// Tries to translate the element in different positions, on-screen and off-screen.
			for (int i = 0; i < 4; i++)
			{
				App.Tap("Stats");
				// Re-query element after tap to avoid stale reference and wait for UI to settle
				var element = App.WaitForElement("Stats");
				ClassicAssert.True(element.GetText()!.StartsWith("Lvl1[0/0]"));
			}
		}
	}
}