#if ANDROID
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

			Thread.Sleep(1000); // Wait some time for the keyboard to appear

			if (App.IsKeyboardShown())
				App.DismissKeyboard();

			VerifyScreenshot();
		}

	}
}
#endif