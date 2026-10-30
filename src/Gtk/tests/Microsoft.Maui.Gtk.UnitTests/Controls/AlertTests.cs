using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>DisplayAlertAsync / DisplayActionSheetAsync / DisplayPromptAsync through the alert delegate convention.</summary>
	public class AlertTests
	{
		static global::Gtk.Window WaitForDialog(ControlsWindowHost host)
		{
			Assert.True(GtkTestHost.PumpUntil(() => GtkDialogs.Active is not null, host.PlatformWindow), "no dialog opened");
			return GtkDialogs.Active!;
		}

		static void Click(global::Gtk.Window dialog, string name)
		{
			var button = GtkTestHost.Find<global::Gtk.Button>(dialog.Child).Single(b => b.Name == name);
			GLib.Signal.Emit(button, "clicked");
		}

		[GtkFact]
		public void AlertReturnsTrueForAcceptAndFalseForCancel() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayAlertAsync("Title", "Message", "Yes", "No");
			var dialog = WaitForDialog(host);

			Assert.True(dialog.Modal);
			Assert.Same(host.PlatformWindow, dialog.TransientFor);
			Assert.Equal("Title", dialog.Title);
			var labels = GtkTestHost.Find<global::Gtk.Label>(dialog.Child).Select(l => l.Text).ToArray();
			Assert.Contains("Title", labels);
			Assert.Contains("Message", labels);

			Click(dialog, "Yes");
			Assert.True(GtkTestHost.Await(task));

			var second = page.DisplayAlertAsync("Title", "Message", "Yes", "No");
			dialog = WaitForDialog(host);
			Click(dialog, "No");
			Assert.False(GtkTestHost.Await(second));
		});

		[GtkFact]
		public void AlertWithoutAcceptOnlyHasCancel() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayAlertAsync("Title", "Info", null, "Close");
			var dialog = WaitForDialog(host);

			Assert.Single(GtkTestHost.Find<global::Gtk.Button>(dialog.Child));
			Click(dialog, "Close");
			Assert.False(GtkTestHost.Await(task));
		});

		[GtkFact]
		public void ClosingTheAlertWindowCancels() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayAlertAsync("Title", "Message", "Yes", "No");
			WaitForDialog(host).Close();

			Assert.False(GtkTestHost.Await(task));
			Assert.Null(GtkDialogs.Active);
		});

		[GtkFact]
		public void ActionSheetReturnsTheChosenButton() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayActionSheetAsync("Pick", "Cancel", "Delete", "One", "Two");
			var dialog = WaitForDialog(host);

			var names = GtkTestHost.Find<global::Gtk.Button>(dialog.Child).Select(b => b.Name).ToArray();
			Assert.Equal(new[] { "Delete", "One", "Two", "Cancel" }, names);
			Assert.True(GtkTestHost.Find<global::Gtk.Button>(dialog.Child).First().HasCssClass("destructive-action"));

			Click(dialog, "Two");
			Assert.Equal("Two", GtkTestHost.Await(task));
		});

		[GtkFact]
		public void ActionSheetCancelAndDismissReturnTheCancelText() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayActionSheetAsync("Pick", "Cancel", null, "One");
			Click(WaitForDialog(host), "Cancel");
			Assert.Equal("Cancel", GtkTestHost.Await(task));

			var dismissed = page.DisplayActionSheetAsync("Pick", "Cancel", null, "One");
			WaitForDialog(host).Close();
			Assert.Equal("Cancel", GtkTestHost.Await(dismissed));
		});

		[GtkFact]
		public void PromptReturnsTheEnteredTextOrNull() => GtkTestHost.Run(() =>
		{
			var page = new ContentPage();
			using var host = new ControlsWindowHost(page);

			var task = page.DisplayPromptAsync("Name", "Your name?", "OK", "Cancel", "type here", 5, Keyboard.Numeric, "ab");
			var dialog = WaitForDialog(host);

			var entry = GtkTestHost.Find<global::Gtk.Entry>(dialog.Child).Single();
			Assert.Equal("ab", entry.Text);
			Assert.Equal("type here", entry.PlaceholderText);
			Assert.Equal(5, entry.MaxLength);
			Assert.Equal(global::Gtk.InputPurpose.Number, entry.InputPurpose);

			entry.Text = "12345";
			Click(dialog, "OK");
			Assert.Equal("12345", GtkTestHost.Await(task));

			var cancelled = page.DisplayPromptAsync("Name", "Your name?");
			Click(WaitForDialog(host), "Cancel");
			Assert.Null(GtkTestHost.Await(cancelled));
		});
	}
}
