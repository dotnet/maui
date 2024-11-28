using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class AlertsGalleryTests : CoreGalleryBasePageTest
	{
		public AlertsGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery("Alerts Gallery");
		}

		// TODO: UI testing alert code is not yet implemented on Windows.
#if !WINDOWS
		[Test]
		[Category(UITestCategories.DisplayAlert)]
		public void AlertCancel()
		{
			var test = Test.Alerts.AlertCancel;

			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			ClassicAssert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Alert Title Here");
			CollectionAssert.Contains(alertText, "Alert Message Here");

			var buttons = alert.GetAlertButtons();
			CollectionAssert.IsNotEmpty(buttons);
			ClassicAssert.True(buttons.Count == 1, $"Expected 1 buttonText, found {buttons.Count}.");

			var cancel = buttons.First();
			ClassicAssert.AreEqual("CANCEL", cancel.GetText());

			cancel.Click();

			App.WaitForNoElement(() => App.GetAlert());

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
		}

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[TestCase(Test.Alerts.AlertAcceptCancelClickAccept, "ACCEPT")]
		[TestCase(Test.Alerts.AlertAcceptCancelClickCancel, "CANCEL")]
		public void AlertAcceptCancel(Test.Alerts test, string buttonText)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			ClassicAssert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Alert Title Here");
			CollectionAssert.Contains(alertText, "Alert Message Here");

			var buttons = alert.GetAlertButtons()
				.Select(b => (Element: b, Text: b.GetText()))
				.ToList();
			CollectionAssert.IsNotEmpty(buttons);
			ClassicAssert.True(buttons.Count == 2, $"Expected 2 buttons, found {buttons.Count}.");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "ACCEPT");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "CANCEL");

			var button = buttons.Single(b => b.Text == buttonText);
			button.Element.Click();

			App.WaitForNoElement(() => App.GetAlert());

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
		}

		[Test]
		[Category(UITestCategories.ActionSheet)]
		[TestCase(Test.Alerts.ActionSheetClickItem, "ITEM 2")]
		[TestCase(Test.Alerts.ActionSheetClickCancel, "CANCEL")]
		[TestCase(Test.Alerts.ActionSheetClickDestroy, "DESTROY")]
		public void ActionSheetClickItem(Test.Alerts test, string itemText)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			ClassicAssert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Action Sheet Title Here");

			var buttons = alert.GetAlertButtons()
				.Select(b => (Element: b, Text: b.GetText()))
				.ToList();
			CollectionAssert.IsNotEmpty(buttons);
			ClassicAssert.True(buttons.Count >= 4 && buttons.Count <= 5, $"Expected 4 or 5 buttons, found {buttons.Count}.");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "DESTROY");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "ITEM 1");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "ITEM 2");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "ITEM 3");

			// handle the case where the dismiss button is an actual button
			if (buttons.Count == 5)
				CollectionAssert.Contains(buttons.Select(b => b.Text), "CANCEL");

			if (buttons.Count == 4 && itemText == "CANCEL")
			{
				// handle the case where the dismiss button is a "click outside the popup"

				alert.DismissAlert();
			}
			else
			{
				// handle the case where the dismiss button is an actual button

				var button = buttons.Single(b => b.Text == itemText);
				button.Element.Click();
			}

			App.WaitForNoElement(() => App.GetAlert());

			var textAfterClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
		}
#endif
	}
}
