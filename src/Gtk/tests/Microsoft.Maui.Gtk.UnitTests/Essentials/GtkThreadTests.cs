using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Media;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

/// <summary>
/// Tests that need a live GTK 4 runtime and display. They run on the <see cref="GtkHost"/> thread and are skipped
/// (or failed, with <c>MAUI_GTK_REQUIRE_NATIVE</c>) when GTK cannot be initialized.
/// </summary>
[Collection("Gtk")]
public class GtkThreadTests
{
	// Points the Essentials dispatcher at the shared GTK thread before any test body runs (skipped tests never construct the class).
	public GtkThreadTests() => _ = GtkHost.Instance;

	[GtkFact]
	public void DeviceDisplayReportsAMonitor()
	{
		var host = GtkHost.Instance!;
		var display = new GtkDeviceDisplay();

		var info = host.Invoke(() => display.MainDisplayInfo);

		Assert.True(info.Width > 0, "width");
		Assert.True(info.Height > 0, "height");
		Assert.True(info.Density >= 1, "density");
	}

	[GtkFact]
	public async Task ClipboardRoundTripsText()
	{
		var clipboard = new GtkClipboard();
		var text = "maui-gtk-" + Guid.NewGuid().ToString("N");

		await clipboard.SetTextAsync(text);

		Assert.True(clipboard.HasText);
		Assert.Equal(text, await clipboard.GetTextAsync().WaitAsync(TimeSpan.FromSeconds(10)));
	}

	[GtkFact]
	public async Task ClipboardChangeEventFires()
	{
		var clipboard = new GtkClipboard();
		var fired = new TaskCompletionSource();
		EventHandler<EventArgs> handler = (_, _) => fired.TrySetResult();
		clipboard.ClipboardContentChanged += handler;
		try
		{
			await clipboard.SetTextAsync("changed-" + Guid.NewGuid().ToString("N"));
			await fired.Task.WaitAsync(TimeSpan.FromSeconds(10));
		}
		finally
		{
			clipboard.ClipboardContentChanged -= handler;
		}
	}

	[GtkFact]
	public void ScreenReaderAnnounceWithoutWindowDoesNotThrow() =>
		new GtkSemanticScreenReader().Announce("hello");

	[GtkFact]
	public async Task ScreenshotCapturesAShownWindow()
	{
		var host = GtkHost.Instance!;
		global::Gtk.Window? window = null;
		host.Invoke(() =>
		{
			window = new global::Gtk.Window { DefaultWidth = 200, DefaultHeight = 100 };
			window.Child = new global::Gtk.Label("screenshot");
			window.Present();
		});

		try
		{
			IScreenshotResult? shot = null;
			for (var attempt = 0; attempt < 50 && shot is null; attempt++)
			{
				await Task.Delay(100);
				shot = await GtkScreenshot.CaptureAsync(window!);
			}

			Assert.NotNull(shot);
			Assert.True(shot!.Width > 0);
			Assert.True(shot.Height > 0);

			using var png = await shot.OpenReadAsync();
			var header = new byte[8];
			Assert.Equal(8, await png.ReadAsync(header, 0, 8));
			Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, header);
		}
		finally
		{
			host.Invoke(() => window!.Destroy());
		}
	}

	[Fact]
	public async Task NonWidgetViewsAreNotCapturable() =>
		Assert.Null(await GtkScreenshot.CaptureAsync(new object()));
}
