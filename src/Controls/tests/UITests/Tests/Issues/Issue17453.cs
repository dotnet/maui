using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	class Issue17453 : _IssuesUITest
	{
		public Issue17453(TestDevice device) : base(device) { }

		public override string Issue => "Clear Entry text tapping the clear button not working";

		[Test]
		public void EntryClearButtonWorks()
		{
			// https://github.com/dotnet/maui/issues/17453
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Windows
			});

			if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
			{
				throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
			}

			App.WaitForElement("WaitForStubControl");
			var rtlEntryRect = App.FindElement("RtlEntry").GetRect();

			// Set focus
			App.Click(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap Clear Button
			var margin = 30;
			App.Click(rtlEntryRect.Width + margin, rtlEntryRect.Y + margin);

			var ltrEntryText = App.FindElement("RtlEntry").GetText();

			Assert.IsEmpty(ltrEntryText);
		}
	}
}