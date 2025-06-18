using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25975 : _IssuesUITest
	{
		public Issue25975(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Double tapping Editor control locks app";

		[Test]
		[Category(UITestCategories.Editor)]
		public void PerformDoubleTapActionOnEditor()
		{
			App.WaitForElement("DoubleTapEditor");
			App.DoubleTap("DoubleTapEditor");
			App.EnterText("DoubleTapEditor", "Hello");

			App.DoubleTap("DoubleTapEditor");
			App.ClearText("DoubleTapEditor");
			App.EnterText("DoubleTapEditor", "World");

			var editorText = App.FindElement("DoubleTapEditor").GetText();
			Assert.That(editorText, Is.EqualTo("World"));
		}
	}
}
