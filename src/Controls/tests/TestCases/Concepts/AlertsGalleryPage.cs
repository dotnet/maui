using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class AlertsGalleryPage : CoreGalleryBasePage
	{
		protected override void Build()
		{
			// ALERTS

			// Test with a single button alert that can be dismissed by tapping the button
			Add(Test.Alerts.AlertCancel, async t =>
			{
				await DisplayAlert(
					"Alert Title Here",
					"Alert Message Here",
					"CANCEL");
				t.ReportSuccessEvent();
			});

			// Test alert with options to Accept or Cancel, Accept is the correct option
			Add(Test.Alerts.AlertAcceptCancelClickAccept, async t =>
			{
				var result = await DisplayAlert(
					"Alert Title Here",
					"Alert Message Here",
					"ACCEPT", "CANCEL");
				if (result)
					t.ReportSuccessEvent();
				else
					t.ReportFailEvent();
			});

			// Test alert with options to Accept or Cancel, Cancel is the correct option
			Add(Test.Alerts.AlertAcceptCancelClickCancel, async t =>
			{
				var result = await DisplayAlert(
					"Alert Title Here",
					"Alert Message Here",
					"ACCEPT", "CANCEL");
				if (result)
					t.ReportFailEvent();
				else
					t.ReportSuccessEvent();
			});

			// ACTION SHEETS

			// Test action sheet with items and Cancel, Item 2 is the correct option
			Add(Test.Alerts.ActionSheetClickItem, async t =>
			{
				var result = await DisplayActionSheet(
					"Action Sheet Title Here",
					"CANCEL", "DESTROY",
					"ITEM 1", "ITEM 2", "ITEM 3");
				if (result == "ITEM 2")
					t.ReportSuccessEvent();
				else
					t.ReportFailEvent();
			});

			// Test action sheet with items and Cancel, Cancel is the correct option
			Add(Test.Alerts.ActionSheetClickCancel, async t =>
			{
				var result = await DisplayActionSheet(
					"Action Sheet Title Here",
					"CANCEL", "DESTROY",
					"ITEM 1", "ITEM 2", "ITEM 3");
				if (result == "CANCEL")
					t.ReportSuccessEvent();
				else
					t.ReportFailEvent();
			});

			// Test action sheet with items and Cancel, Destroy is the correct option
			Add(Test.Alerts.ActionSheetClickDestroy, async t =>
			{
				var result = await DisplayActionSheet(
					"Action Sheet Title Here",
					"CANCEL", "DESTROY",
					"ITEM 1", "ITEM 2", "ITEM 3");
				if (result == "DESTROY")
					t.ReportSuccessEvent();
				else
					t.ReportFailEvent();
			});
		}

		ExpectedEventViewContainer<Button>
			Add(Test.Alerts test, Func<ExpectedEventViewContainer<Button>, Task> action) =>
			Add(new ExpectedEventViewContainer<Button>(test, new Button { Text = "Click Me!" }))
				.With(t => t.View.Clicked += (_, _) => action(t));
	}
}
