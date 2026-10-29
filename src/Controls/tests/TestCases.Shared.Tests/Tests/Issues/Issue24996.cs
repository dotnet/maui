using NUnit.Framework;
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
				bool completed = App.WaitForTextToBePresentInElement("Stats", $"Step {i + 1}:");
				Assert.That(completed, Is.True, $"Translation step {i + 1} did not complete.");
				Assert.That(App.FindElement("Stats").GetText(), Does.Contain("Lvl1[0/0]"));
			}
		}
	}
}