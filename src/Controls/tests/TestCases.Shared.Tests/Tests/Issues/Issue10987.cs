using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10987 : _IssuesUITest
	{
		public Issue10987(TestDevice device) : base(device) { }

		public override string Issue => "Editor HorizontalTextAlignment Does not Works.";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorPlaceholderRuntimeTextAlignmentChanged()
		{
			App.WaitForElement("RTLEditor");
			App.Tap("Button");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorRuntimeTextAlignmentChanged()
		{
			App.WaitForElement("RTLEditor");
			App.EnterText("Editor", "Editor Text");
			App.EnterText("RTLEditor", "RTL Editor");
			var app = App as AppiumApp;
			KeyboardScrolling.HideKeyboard(app!, app!.Driver, true);
			App.Tap("ResetButton");
			VerifyScreenshot();
		}
	}
}
#endif