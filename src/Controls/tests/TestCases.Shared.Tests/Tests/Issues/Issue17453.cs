#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue17453 : _IssuesUITest
	{
		public Issue17453(TestDevice device) : base(device) { }

		public override string Issue => "Clear Entry text tapping the clear button not working";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryClearButtonWorksEntryDoesntClearWhenNotClickingOnClear()
		{
			// https://github.com/dotnet/maui/issues/17453

			App.WaitForElement("WaitForStubControl");
			string? rtlEntryText = App.FindElement("RtlEntry").GetText();

			if (String.IsNullOrWhiteSpace(rtlEntryText))
				App.EnterText("RtlEntry", "Simple Text");

			var rtlEntryRect = App.FindElement("RtlEntry").GetRect();
			App.EnterText("RtlEntry", "Simple Text");

			// Set focus
			App.TapCoordinates(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap on the entry but not on the clear button
			App.TapCoordinates(rtlEntryRect.CenterX(), rtlEntryRect.CenterY());

			rtlEntryText = App.FindElement("RtlEntry").GetText();

			ClassicAssert.IsNotEmpty(rtlEntryText);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryClearButtonWorks()
		{
			// https://github.com/dotnet/maui/issues/17453

			App.WaitForElement("WaitForStubControl");

			string? rtlEntryText = App.FindElement("RtlEntry").GetText();

			if (String.IsNullOrWhiteSpace(rtlEntryText))
				App.EnterText("RtlEntry", "Simple Text");

			var rtlEntryRect = App.FindElement("RtlEntry").GetRect();

			// Set focus
			App.TapCoordinates(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap Clear Button
			var margin = 30;
			App.TapCoordinates(rtlEntryRect.X + margin, rtlEntryRect.Y + margin);

			rtlEntryText = App.FindElement("RtlEntry").GetText();

			ClassicAssert.IsEmpty(rtlEntryText);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task EntryWithMarginClearButtonWorks()
		{
			// https://github.com/dotnet/maui/issues/25225

			App.WaitForElement("WaitForStubControl");

			string? entryText = App.FindElement("EntryWithMargin").GetText();
			if (String.IsNullOrWhiteSpace(entryText))
				App.EnterText("EntryWithMargin", "Simple Text");

			var entryRect = App.FindElement("EntryWithMargin").GetRect();

			// Set focus
			App.TapCoordinates(entryRect.Width, entryRect.Y);
			await Task.Delay(500);

			// Tap Clear Button
			App.TapCoordinates(entryRect.Width, entryRect.Y);

			entryText = App.FindElement("EntryWithMargin").GetText();

			ClassicAssert.IsEmpty(entryText);
		}
	}
}
#endif