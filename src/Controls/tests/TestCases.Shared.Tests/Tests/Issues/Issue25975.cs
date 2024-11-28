#if ANDROID
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
			App.WaitForElement("Editor");
			App.DoubleTap("Editor");
			App.EnterText("Editor", "Hello world");
			var editorText = App.FindElement("Editor").GetText();
			Assert.That(editorText, Is.EqualTo("Hello world"));
		}
	}
}
#endif