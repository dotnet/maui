using Microsoft.Maui.Controls;

namespace Maui.HybridApp.UI;

static partial class WindowCenter
{
	internal static bool Supported => OperatingSystem.IsWindows() || OperatingSystem.IsMacCatalyst();
	internal static Microsoft.Maui.Graphics.Point Position(Microsoft.Maui.Graphics.Rect display, Microsoft.Maui.Graphics.Size size)
	{
		if (!double.IsFinite(display.X) || !double.IsFinite(display.Y) ||
			!double.IsFinite(display.Width) || !double.IsFinite(display.Height) ||
			!double.IsFinite(size.Width) || !double.IsFinite(size.Height) ||
			display.Width <= 0 || display.Height <= 0 || size.Width <= 0 || size.Height <= 0)
			throw new CommandException("native_error", "The actual display and window geometry must be positive and finite.");
		return new(display.X + (display.Width - size.Width) / 2, display.Y + (display.Height - size.Height) / 2);
	}
	internal static void Apply(Window window)
	{
#if WINDOWS
		if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native || native.Content?.XamlRoot is null)
			throw new CommandException("context_unavailable", "The owning native desktop window is not attached.");
		var handle = WinRT.Interop.WindowNative.GetWindowHandle(native);
		var monitor = MonitorFromWindow(handle, 2);
		var info = new MonitorInfo { Size = System.Runtime.InteropServices.Marshal.SizeOf<MonitorInfo>() };
		if (monitor == nint.Zero || !GetMonitorInfo(monitor, ref info))
			throw new CommandException("native_error", "The owning window's current display work area could not be read.");
		var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle));
		var scale = native.Content.XamlRoot.RasterizationScale;
		if (!double.IsFinite(scale) || scale <= 0)
			throw new CommandException("native_error", "The native window display density is unavailable.");
		var x = info.Work.Left + (info.Work.Right - info.Work.Left - appWindow.Size.Width) / 2.0;
		var y = info.Work.Top + (info.Work.Bottom - info.Work.Top - appWindow.Size.Height) / 2.0;
		// Move in native pixels as one pair; handler frame notifications convert to
		// logical coordinates using the destination's actual density.
		appWindow.Move(new global::Windows.Graphics.PointInt32((int)Math.Round(x), (int)Math.Round(y)));
#elif MACCATALYST
		if (window.Handler?.PlatformView is not UIKit.UIWindow native || native.WindowScene is not { } scene)
			throw new CommandException("context_unavailable", "The owning native desktop window is not attached.");
		var frame = scene.EffectiveGeometry.SystemFrame;
		if (CGGetActiveDisplayList(0, null, out var count) != 0 || count is 0 or > 64)
			throw new CommandException("native_error", "The native desktop displays could not be enumerated.");
		var displays = new uint[count];
		if (CGGetActiveDisplayList(count, displays, out count) != 0)
			throw new CommandException("native_error", "The native desktop displays could not be read.");
		var center = new Microsoft.Maui.Graphics.Point(frame.X + frame.Width / 2, frame.Y + frame.Height / 2);
		var bounds = displays.Take((int)count).Select(id => CGDisplayBounds(id))
			.Select(rect => new Microsoft.Maui.Graphics.Rect(rect.X, rect.Y, rect.Width, rect.Height))
			.OrderBy(rect => Math.Pow(Math.Max(rect.Left - center.X, Math.Max(0, center.X - rect.Right)), 2) +
				Math.Pow(Math.Max(rect.Top - center.Y, Math.Max(0, center.Y - rect.Bottom)), 2))
			.First();
		var position = Position(bounds, new(frame.Width, frame.Height));
		window.X = position.X;
		window.Y = position.Y;
#else
		throw new NotSupportedException("Native window centering is supported on desktop hosts only.");
#endif
	}
#if MACCATALYST
	[System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	static extern int CGGetActiveDisplayList(uint maximum, [System.Runtime.InteropServices.Out] uint[]? displays, out uint count);
	[System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
	static extern CoreGraphics.CGRect CGDisplayBounds(uint display);
#endif
#if WINDOWS
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct NativeRect { internal int Left, Top, Right, Bottom; }
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct MonitorInfo { internal int Size; internal NativeRect Monitor, Work; internal int Flags; }
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	static extern nint MonitorFromWindow(nint window, uint flags);
	[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
	[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
	static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);
#endif
}
