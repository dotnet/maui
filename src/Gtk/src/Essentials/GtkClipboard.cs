using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IClipboard"/> over the default display's <c>Gdk.Clipboard</c>. Reads are asynchronous
/// (GDK may have to ask the owning process for the data), writes take effect immediately.
/// </summary>
/// <remarks>
/// Needs an initialized GTK (a default <c>Gdk.Display</c>); calls are marshalled to the GTK main thread.
/// <see cref="ClipboardContentChanged"/> is raised on that thread.
/// </remarks>
sealed class GtkClipboard : IClipboard
{
	EventHandler<EventArgs>? changed;
	EventHandler? nativeHandler;
	Gdk.Clipboard? subscribed;
	int subscribers;

	public bool HasText =>
		GtkDispatch.Invoke(() =>
			Clipboard()?.Formats?.ContainMimeType("text/plain;charset=utf-8") == true
			|| Clipboard()?.Formats?.ContainMimeType("text/plain") == true
			|| Clipboard()?.Formats?.ContainMimeType("UTF8_STRING") == true);

	public Task SetTextAsync(string? text) =>
		GtkDispatch.InvokeAsync(() =>
		{
			var clipboard = Clipboard() ?? throw new InvalidOperationException("No GTK display is available for the clipboard.");
			if (text is null)
				clipboard.SetContent(null!);
			else
				clipboard.Text = text;
		});

	public Task<string?> GetTextAsync() =>
		GtkAsync.Begin<Gdk.Clipboard, string?>(
			callback =>
			{
				var clipboard = Clipboard() ?? throw new InvalidOperationException("No GTK display is available for the clipboard.");
				clipboard.ReadTextAsync(null!, callback);
				return clipboard;
			},
			(clipboard, result) => clipboard.ReadTextFinish(result));

	public event EventHandler<EventArgs> ClipboardContentChanged
	{
		add
		{
			lock (this)
			{
				changed += value;
				if (subscribers++ == 0)
					GtkDispatch.Invoke(Subscribe);
			}
		}

		remove
		{
			lock (this)
			{
				changed -= value;
				if (subscribers > 0 && --subscribers == 0)
					GtkDispatch.Invoke(Unsubscribe);
			}
		}
	}

	static Gdk.Clipboard? Clipboard() => Gdk.Display.Default?.Clipboard;

	void Subscribe()
	{
		subscribed = Clipboard();
		if (subscribed is null)
			return;

		nativeHandler = (_, _) => changed?.Invoke(this, EventArgs.Empty);
		subscribed.Changed += nativeHandler;
	}

	void Unsubscribe()
	{
		if (subscribed is not null && nativeHandler is not null)
			subscribed.Changed -= nativeHandler;

		subscribed = null;
		nativeHandler = null;
	}
}
