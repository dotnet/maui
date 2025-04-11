#if TEST_FAILS_ON_IOS //For more info see: https://github.com/dotnet/maui/issues/28806
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue19509 : _IssuesUITest
	{
		public Issue19509(TestDevice device) : base(device)
		{
		}

		public override string Issue => "The entry text color property not working when the text value is bound after some time";

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task EntryTextColorStopsWorkingAfterPropertyIsUpdatedFromBinding()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Click a button to update the text
			App.Tap("button");

			await Task.Delay(1000); // Wait for Ripple Effect animation to complete.

			// 2. Verify that the Entry TextColor is correct (Green).
			VerifyScreenshot();
		}
	}
}
#endif