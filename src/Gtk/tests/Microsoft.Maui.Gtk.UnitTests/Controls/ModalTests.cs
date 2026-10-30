using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Controls.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Modal pages are presented as modal windows transient for the page below.</summary>
	public class ModalTests
	{
		static ContentPage Page(string title, string text) => new()
		{
			Title = title,
			Content = new Label { Text = text, AutomationId = text },
		};

		[GtkFact]
		public void PushModalOpensAModalWindowThatLaysTheModalPageOut() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "root");
			var modal = Page("Modal", "modal-label");
			using var host = new ControlsWindowHost(root);

			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(modal, false));

			var window = GtkModalNavigationBackend.WindowOf(modal);
			Assert.NotNull(window);
			Assert.True(window!.Modal);
			Assert.Same(host.PlatformWindow, window.TransientFor);
			Assert.Equal("Modal", window.Title);
			Assert.True(window.Visible);

			GtkTestHost.Resize(window, 400, 300);
			var label = (global::Gtk.Widget)modal.Content.Handler!.PlatformView!;
			Assert.True(label.Width > 0 && label.Height > 0);
			GtkTestHost.Retire(window);
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PopModalClosesTheWindowAndDisconnectsThePage() => GtkTestHost.Run(() =>
		{
			var modal = Page("Modal", "modal-label");
			using var host = new ControlsWindowHost(Page("Root", "root"));

			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(modal, false));
			var window = GtkModalNavigationBackend.WindowOf(modal)!;
			Assert.Single(host.Window.Navigation.ModalStack);

			var popped = GtkTestHost.Await(host.Window.Navigation.PopModalAsync(false));

			Assert.Same(modal, popped);
			Assert.Empty(host.Window.Navigation.ModalStack);
			Assert.Null(GtkModalNavigationBackend.WindowOf(modal));
			Assert.False(window.Visible);
			Assert.Null(modal.Handler);
		});

		[GtkFact]
		public void ModalsStackAndTheTopOneIsPoppedFirst() => GtkTestHost.Run(() =>
		{
			var first = Page("First", "one");
			var second = Page("Second", "two");
			using var host = new ControlsWindowHost(Page("Root", "root"));

			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(first, false));
			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(second, false));

			var firstWindow = GtkModalNavigationBackend.WindowOf(first)!;
			var secondWindow = GtkModalNavigationBackend.WindowOf(second)!;
			Assert.Same(firstWindow, secondWindow.TransientFor);

			GtkTestHost.Await(host.Window.Navigation.PopModalAsync(false));
			Assert.Null(GtkModalNavigationBackend.WindowOf(second));
			Assert.NotNull(GtkModalNavigationBackend.WindowOf(first));

			GtkTestHost.Await(host.Window.Navigation.PopModalAsync(false));
			Assert.Null(GtkModalNavigationBackend.WindowOf(first));
		});

		[GtkFact]
		public void ClosingTheModalWindowPopsThePage() => GtkTestHost.Run(() =>
		{
			var modal = Page("Modal", "modal-label");
			using var host = new ControlsWindowHost(Page("Root", "root"));
			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(modal, false));

			var window = GtkModalNavigationBackend.WindowOf(modal)!;
			window.Close();

			Assert.True(GtkTestHost.PumpUntil(() => host.Window.Navigation.ModalStack.Count == 0, host.PlatformWindow));
			Assert.Null(GtkModalNavigationBackend.WindowOf(modal));
		});

		[GtkFact]
		public void AModalNavigationPageGetsItsOwnTitleBar() => GtkTestHost.Run(() =>
		{
			var modal = new NavigationPage(Page("Inner title", "inner"));
			using var host = new ControlsWindowHost(Page("Root", "root"));
			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(modal, false));

			var window = GtkModalNavigationBackend.WindowOf(modal)!;
			Assert.True(GtkTestHost.PumpUntil(() => window.Titlebar is global::Gtk.HeaderBar, host.PlatformWindow));

			var toolbar = (GtkControlsToolbarHandler)modal.Toolbar!.Handler!;
			Assert.Same(toolbar.HeaderBar, window.Titlebar);
			Assert.Equal("Inner title", toolbar.TitleLabel.Text);

			// The application window keeps its own title bar (none here).
			Assert.NotSame(toolbar.HeaderBar, host.PlatformWindow.Titlebar);
		});

		[GtkFact]
		public void DialogsOfAModalPageAreTransientForTheModalWindow() => GtkTestHost.Run(() =>
		{
			var modal = Page("Modal", "modal-label");
			using var host = new ControlsWindowHost(Page("Root", "root"));
			GtkTestHost.Await(host.Window.Navigation.PushModalAsync(modal, false));

			var task = modal.DisplayAlertAsync("T", "M", "OK", "Cancel");
			Assert.True(GtkTestHost.PumpUntil(() => GtkDialogs.Active is not null, host.PlatformWindow));
			var dialog = GtkDialogs.Active!;

			Assert.Same(GtkModalNavigationBackend.WindowOf(modal), dialog.TransientFor);

			GLib.Signal.Emit(GtkTestHost.Find<global::Gtk.Button>(dialog.Child).Single(b => b.Name == "OK"), "clicked");
			Assert.True(GtkTestHost.Await(task));
		});
	}
}
