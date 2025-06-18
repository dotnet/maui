using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18443 : _IssuesUITest
	{
		public Issue18443(TestDevice device) : base(device) { }

		public override string Issue => "SelectionLength Property Not Applied to Entry at Runtime";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntrySelectionLengthRuntimeUpdate()
		{
			App.WaitForElement("entry");
			Thread.Sleep(1000);
#if ANDROID
			if (App.IsKeyboardShown())
				App.DismissKeyboard();
#endif
			VerifyScreenshot();
		}

	}
}