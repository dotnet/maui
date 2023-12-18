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
		public void EntryClearButtonWorksEntryDoesntClearWhenNotClickingOnClear()
		{
			// https://github.com/dotnet/maui/issues/17453
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Windows
			});

			App.WaitForElement("WaitForStubControl");
			var rtlEntryRect = App.FindElement("RtlEntry").GetRect();
			App.EnterText("RtlEntry", "Simple Text");

			// Set focus
			App.Click(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap on the entry but not on the clear button
			App.Click(rtlEntryRect.CenterX(), rtlEntryRect.CenterY());

			string? ltrEntryText = App.FindElement("RtlEntry").GetText();

			Assert.IsNotEmpty(ltrEntryText);
		}

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

			App.WaitForElement("WaitForStubControl");
			App.EnterText("RtlEntry", "Simple Text");
			var rtlEntryRect = App.FindElement("RtlEntry").GetRect();

			// Set focus
			App.Click(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap Clear Button
			var margin = 30;
			App.Click(rtlEntryRect.Width + margin, rtlEntryRect.Y + margin);

			string? ltrEntryText = App.FindElement("RtlEntry").GetText();

			Assert.IsEmpty(ltrEntryText);
		}
	}
}