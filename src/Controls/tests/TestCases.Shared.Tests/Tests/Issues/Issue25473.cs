using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25473 : _IssuesUITest
	{
		public Issue25473(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "MAUI Entry in Windows always shows ClearButton despite ClearButtonVisibility set to Never";

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryClearButtonVisibilitySetToWhileEditing()
		{
			App.WaitForElement("ToggleClearButtonVisibilityButton");
			App.Click("ToggleClearButtonVisibilityButton");
			App.ClearText("MainEntryField");
			App.EnterText("MainEntryField", "ClearButton is set to WhileEditing");
			App.Tap("MainEntryField");
#if MACCATALYST
			{
				// On MacCatalyst, pressing the ESC key during screenshot capture clears the text.
			// This causes the image generated in CI to differ from local runs.
			Assert.That(App.WaitForElement("ClearButton is set to WhileEditing").GetText(), Is.EqualTo("ClearButton is set to WhileEditing"));
			}
#else
			{
				VerifyScreenshot();
			}
#endif
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryClearButtonVisibilitySetToNever()
		{
			App.WaitForElement("ToggleClearButtonVisibilityButton");
			App.Click("ToggleClearButtonVisibilityButton");
			App.ClearText("MainEntryField");
			App.EnterText("MainEntryField", "ClearButton is set to Never");
			App.Tap("MainEntryField");
#if MACCATALYST
			{
				// On MacCatalyst, pressing the ESC key during screenshot capture clears the text.
			// This causes the image generated in CI to differ from local runs.
			Assert.That(App.WaitForElement("ClearButton is set to Never").GetText(), Is.EqualTo("ClearButton is set to Never"));
			}
#else
			{
				VerifyScreenshot();
			}
#endif
		}
	}
}