using Xunit;
using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.DisplayAlert)]
		public void AlertCancel()
		{
			var test = Test.Alerts.AlertCancel;

			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.Equal($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			Assert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Alert Title Here");
			CollectionAssert.Contains(alertText, "Alert Message Here");

			var buttons = alert.GetAlertButtons();
			CollectionAssert.NotEmpty(buttons);
			Assert.True(buttons.Count == 1, $"Expected 1 buttonText, found {buttons.Count}.");

			var cancel = buttons.First();
			Assert.Equal("CANCEL", cancel.GetText();

			cancel.Click();

			App.WaitForNoElement(() => App.GetAlert());

			var textAfterClick = remote.GetEventLabel().GetText();
			Assert.Equal($"Event: {test} (SUCCESS 1)", textAfterClick);
		}

		[Fact]
		[Trait("Category", UITestCategories.DisplayAlert)]
		[Theory]
		[InlineData(Test.Alerts.AlertAcceptCancelClickAccept, "ACCEPT")]
		[Theory]
		[InlineData(Test.Alerts.AlertAcceptCancelClickCancel, "CANCEL")]
		public void AlertAcceptCancel(Test.Alerts test, string buttonText)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.Equal($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			Assert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Alert Title Here");
			CollectionAssert.Contains(alertText, "Alert Message Here");

			var buttons = alert.GetAlertButtons()
				.Select(b => (Element: b, Text: b.GetText()))
				.ToList();
			CollectionAssert.NotEmpty(buttons);
			Assert.True(buttons.Count == 2, $"Expected 2 buttons, found {buttons.Count}.");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "ACCEPT");
			CollectionAssert.Contains(buttons.Select(b => b.Text), "CANCEL");

			var button = buttons.Single(b => b.Text == buttonText);
			button.Element.Click();

			App.WaitForNoElement(() => App.GetAlert());

			var textAfterClick = remote.GetEventLabel().GetText();
			Assert.Equal($"Event: {test} (SUCCESS 1)", textAfterClick);
		}

		[Fact]
		[Trait("Category", UITestCategories.ActionSheet)]
		[Theory]
		[InlineData(Test.Alerts.ActionSheetClickItem, "ITEM 2")]
		[Theory]
		[InlineData(Test.Alerts.ActionSheetClickCancel, "CANCEL")]
		[Theory]
		[InlineData(Test.Alerts.ActionSheetClickDestroy, "DESTROY")]
		public void ActionSheetClickItem(Test.Alerts test, string itemText)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.Equal($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var alert = App.WaitForElement(() => App.GetAlert());
			Assert.NotNull(alert);

			var alertText = alert.GetAlertText();
			CollectionAssert.Contains(alertText, "Action Sheet Title Here");

			var buttons = alert.GetAlertButtons()
				.Select(b => (Element: b, Text: b.GetText()))
				.ToList();
			CollectionAssert.NotEmpty(buttons);
			Assert.True(buttons.Count >= 4 && buttons.Count <= 5, $"Expected 4 or 5 buttons, found {buttons.Count}.");
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
			Assert.Equal($"Event: {test} (SUCCESS 1)", textAfterClick);
		}
	}
}
