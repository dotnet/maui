using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IDeviceDisplay"/> over <c>Gdk.Display.Monitors</c>. GTK 4 has no primary monitor, so the first monitor of the
/// default display is reported. Sizes are in physical pixels (logical geometry multiplied by the monitor scale).
/// </summary>
/// <remarks>
/// <see cref="KeepScreenOn"/> inhibits idle through <c>Gtk.Application.Inhibit</c> and therefore needs the running
/// <c>Gtk.Application</c> and a window; if they are not available yet the request is applied as soon as it can be, on the next
/// change of the value or of the display configuration.
/// </remarks>
sealed class GtkDeviceDisplay : IDeviceDisplay
{
	readonly object gate = new();
	EventHandler<DisplayInfoChangedEventArgs>? changed;
	GLib.IListModel? monitors;
	GLib.ItemsChangedHandler? monitorsHandler;
	int subscribers;
	bool keepScreenOn;
	uint inhibitCookie;
	global::Gtk.Application? inhibitApp;

	public DisplayInfo MainDisplayInfo => GtkDispatch.Invoke(ReadMainDisplay);

	public bool KeepScreenOn
	{
		get => keepScreenOn;
		set
		{
			if (keepScreenOn == value)
				return;

			keepScreenOn = value;
			GtkDispatch.Invoke(ApplyInhibit);
		}
	}

	public event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
	{
		add
		{
			lock (gate)
			{
				changed += value;
				if (subscribers++ == 0)
					GtkDispatch.Invoke(Subscribe);
			}
		}

		remove
		{
			lock (gate)
			{
				changed -= value;
				if (subscribers > 0 && --subscribers == 0)
					GtkDispatch.Invoke(Unsubscribe);
			}
		}
	}

	/// <summary>Orientation is landscape when the width is larger than the height.</summary>
	internal static DisplayInfo Create(double logicalWidth, double logicalHeight, double scale, float refreshHz)
	{
		var orientation = logicalWidth > logicalHeight ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
		return new DisplayInfo(logicalWidth * scale, logicalHeight * scale, scale, orientation, DisplayRotation.Rotation0, refreshHz);
	}

	static DisplayInfo ReadMainDisplay()
	{
		var list = Gdk.Display.Default?.Monitors;
		if (list is null || list.NItems == 0 || list.GetObject(0) is not Gdk.Monitor monitor)
			return new DisplayInfo(0, 0, 1, DisplayOrientation.Unknown, DisplayRotation.Unknown, 0);

		var geometry = monitor.Geometry;
		var scale = monitor.Scale > 0 ? monitor.Scale : Math.Max(1, monitor.ScaleFactor);
		return Create(geometry.Width, geometry.Height, scale, monitor.RefreshRate / 1000f);
	}

	void Subscribe()
	{
		monitors = Gdk.Display.Default?.Monitors;
		if (monitors is null)
			return;

		monitorsHandler = (_, _) =>
		{
			ApplyInhibit();
			changed?.Invoke(this, new DisplayInfoChangedEventArgs(ReadMainDisplay()));
		};
		monitors.ItemsChanged += monitorsHandler;
	}

	void Unsubscribe()
	{
		if (monitors is not null && monitorsHandler is not null)
			monitors.ItemsChanged -= monitorsHandler;

		monitors = null;
		monitorsHandler = null;
	}

	void ApplyInhibit()
	{
		if (keepScreenOn && inhibitCookie == 0)
		{
			if (GLib.Application.Default is global::Gtk.Application app)
			{
				inhibitCookie = app.Inhibit(app.ActiveWindow!, global::Gtk.ApplicationInhibitFlags.Idle, "Keep screen on");
				inhibitApp = app;
			}
		}
		else if (!keepScreenOn && inhibitCookie != 0)
		{
			inhibitApp?.Uninhibit(inhibitCookie);
			inhibitCookie = 0;
			inhibitApp = null;
		}
	}
}
