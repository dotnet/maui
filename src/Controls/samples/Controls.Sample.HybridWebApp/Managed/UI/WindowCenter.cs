using Microsoft.Maui.Controls;

namespace Maui.HybridApp.UI;

static partial class WindowCenter
{
	internal static bool Supported => OperatingSystem.IsWindows();
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
#else
		throw new NotSupportedException("Current-display usable-work-area centering is implemented on Windows only; no primary-monitor approximation is used.");
#endif
	}
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
