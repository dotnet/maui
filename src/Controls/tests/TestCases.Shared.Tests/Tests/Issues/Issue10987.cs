using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10987 : _IssuesUITest
	{
		const string Editor = "Editor";
		const string RTLEditor = "RTLEditor";
		public Issue10987(TestDevice device) : base(device) { }

		public override string Issue => "Editor HorizontalTextAlignment Does not Works.";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorPlaceholderRuntimeTextAlignmentChanged()
		{
			App.WaitForElement(RTLEditor);
			App.Tap("Button");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorRuntimeTextAlignmentChanged()
		{
			//To make sure editor is focused in mac platform
			App.Tap(Editor);
			App.EnterText(Editor, "Editor Text");
			App.WaitForElement(RTLEditor);
			App.EnterText(RTLEditor, "RTL Editor");
#if IOS
			var app = App as AppiumApp;
			KeyboardScrolling.HideKeyboard(app!, app!.Driver, true);
#endif
			App.Tap("ResetButton");
			VerifyScreenshot();
		}
	}
}