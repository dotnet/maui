﻿// #if ANDROID
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
			string? rtlEntryText = App.WaitForElement("RtlEntry").GetText();

			if (String.IsNullOrWhiteSpace(rtlEntryText))
				App.EnterText("RtlEntry", "Simple Text");

			var rtlEntryRect = App.WaitForElement("RtlEntry").GetRect();
			App.EnterText("RtlEntry", "Simple Text");

			// Set focus
			App.TapCoordinates(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap on the entry but not on the clear button
			App.TapCoordinates(rtlEntryRect.CenterX(), rtlEntryRect.CenterY());

			rtlEntryText = App.WaitForElement("RtlEntry").GetText();

			ClassicAssert.IsNotEmpty(rtlEntryText);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryClearButtonWorks()
		{
			// https://github.com/dotnet/maui/issues/17453

			App.WaitForElement("WaitForStubControl");

			string? rtlEntryText = App.WaitForElement("RtlEntry").GetText();

			if (String.IsNullOrWhiteSpace(rtlEntryText))
				App.EnterText("RtlEntry", "Simple Text");

			var rtlEntryRect = App.WaitForElement("RtlEntry").GetRect();

			// Set focus
			App.TapCoordinates(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap Clear Button
			var margin = 30;
			App.TapCoordinates(rtlEntryRect.X + margin, rtlEntryRect.Y + margin);

			rtlEntryText = App.WaitForElement("RtlEntry").GetText();

			ClassicAssert.IsEmpty(rtlEntryText);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task EntryWithMarginClearButtonWorks()
		{
			// https://github.com/dotnet/maui/issues/25225

			App.WaitForElement("WaitForStubControl");

			string? entryText = App.WaitForElement("EntryWithMargin").GetText();
			if (String.IsNullOrWhiteSpace(entryText))
				App.EnterText("EntryWithMargin", "Simple Text");

			var entryRect = App.WaitForElement("EntryWithMargin").GetRect();

			// Set focus
			App.TapCoordinates(entryRect.Width-2, entryRect.Y);
			await Task.Delay(500);

			// Tap Clear Button
			App.TapCoordinates(725, entryRect.Y);
			App.TapCoordinates(entryRect.Width-2, entryRect.Y);

			entryText = App.WaitForElement("EntryWithMargin").GetText();

			ClassicAssert.IsEmpty(entryText);
		}
	}
}
// #endif