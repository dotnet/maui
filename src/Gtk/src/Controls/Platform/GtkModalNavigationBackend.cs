using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Platform;
using GtkWindow = Gtk.Window;

namespace Microsoft.Maui.Controls.Gtk.Platform
{
	/// <summary>
	/// Presents modal pages (<c>Navigation.PushModalAsync</c>) as modal <c>Gtk.Window</c>s transient for the window of the page below,
	/// sized like it. The modal page gets its own window scope, so its toolbar becomes the modal window's title bar and its dialogs
	/// are transient for the modal window.
	/// </summary>
	/// <remarks>
	/// A native modal window is the GTK 4 idiom (no overlay layer in the parent window is needed, focus and Escape behave as users
	/// expect). Closing the modal window with the window manager pops the page. <c>animated</c> is ignored: the window manager animates.
	/// </remarks>
	sealed class GtkModalNavigationBackend : IModalNavigationBackend
	{
		/// <summary>The singleton installed into <see cref="ModalNavigationBackend.Current"/>.</summary>
		public static GtkModalNavigationBackend Instance { get; } = new();

		/// <summary>The modal windows currently open, by page; for tests.</summary>
		internal static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Page, ModalEntry> Entries = new();

		internal sealed class ModalEntry
		{
			public ModalEntry(GtkWindow window, MauiGtkFillHost host, IDisposable scope)
			{
				Window = window;
				Host = host;
				Scope = scope;
			}

			public GtkWindow Window { get; }
			public MauiGtkFillHost Host { get; }
			public IDisposable Scope { get; }
		}

		/// <summary>The modal window of <paramref name="page"/>, or <see langword="null"/> when it is not shown modally.</summary>
		/// <param name="page">The modal page.</param>
		internal static GtkWindow? WindowOf(Page page) => Entries.TryGetValue(page, out var entry) ? entry.Window : null;

		/// <inheritdoc/>
		public Task PushAsync(Controls.Window window, Page modal, Page previous, bool animated)
		{
			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			GtkRuntime.Post(() =>
			{
				try
				{
					Show(window, modal, previous);
					tcs.SetResult(true);
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});

			return tcs.Task;
		}

		/// <inheritdoc/>
		public Task PopAsync(Controls.Window window, Page popped, Page next, bool animated)
		{
			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			GtkRuntime.Post(() =>
			{
				try
				{
					Hide(popped);
					tcs.SetResult(true);
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});

			return tcs.Task;
		}

		static void Show(Controls.Window window, Page modal, Page previous)
		{
			var parentContext = previous.Handler?.MauiContext ?? window.Handler?.MauiContext
				?? throw new InvalidOperationException("The window has no MauiContext to show a modal page in.");

			var parent = parentContext.Services.GetService<GtkWindow>() ?? window.Handler?.PlatformView as GtkWindow;

			var platformWindow = new GtkWindow
			{
				Modal = true,
				Title = modal.Title ?? string.Empty,
			};

			if (parent is not null)
			{
				platformWindow.TransientFor = parent;
				platformWindow.SetDefaultSize(Math.Max(parent.Width, 320), Math.Max(parent.Height, 240));
			}
			else
			{
				platformWindow.SetDefaultSize(800, 600);
			}

			var context = parentContext.MakeWindowScope(platformWindow, out var scope);

			// The window scope registers the window as `object` on the platform-less build; handlers look for the Gtk.Window.
			((MauiContext)context).AddWeakSpecific<GtkWindow>(platformWindow);
			var host = new MauiGtkFillHost();
			platformWindow.Child = host;
			host.SetView(modal, context);

			// Only an explicit toolbar (a NavigationPage's, or one the page set) replaces the plain window title bar.
			if (modal.Toolbar is { } toolbar)
				platformWindow.Titlebar = (global::Gtk.Widget)toolbar.ToPlatform(context);

			Entries.Add(modal, new ModalEntry(platformWindow, host, scope));

			platformWindow.CloseRequest += (_, args) =>
			{
				// The window manager closed the window: ask the navigation to pop it; the pop closes the window.
				args.RetVal = Entries.TryGetValue(modal, out var _entry);
				if ((bool)args.RetVal && window.Navigation.ModalStack.Count > 0 && ReferenceEquals(window.Navigation.ModalStack[window.Navigation.ModalStack.Count - 1], modal))
					_ = window.Navigation.PopModalAsync(true);
			};

			platformWindow.Present();
		}

		static void Hide(Page popped)
		{
			if (!Entries.TryGetValue(popped, out var entry))
				return;

			Entries.Remove(popped);

			entry.Window.Titlebar = null;
			entry.Host.Clear();

			// The page stays alive for the application; its handler belongs to the modal scope that is going away.
			popped.Handler?.DisconnectHandler();

			entry.Window.Visible = false;
			entry.Window.TransientFor = null;
			entry.Window.Close();
			entry.Scope.Dispose();
		}
	}
}
