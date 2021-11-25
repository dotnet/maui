using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public static class HardwareInformations {

		public static Gdk.Screen DefaultScreen => Gdk.Screen.Default;

		/// <summary>
		/// A <see cref="Gdk.Visual"/> describes a particular video hardware display format.
		/// It includes information about the number of bits used for each color, the way the bits are translated into an RGB value for display,
		/// and the way the bits are stored in memory. For example, a piece of display hardware might support 24-bit color, 16-bit color, or 8-bit color;
		/// meaning 24/16/8-bit pixel sizes. For a given pixel size, pixels can be in different formats;
		/// for example the “red” element of an RGB pixel may be in the top 8 bits of the pixel, or may be in the lower 4 bits.
		/// There are several standard visuals.
		/// The visual returned by <see cref="Gdk.Screen.Default.SystemVisual"/> is the system’s default visual,
		/// and the visual returned by <see cref="Gdk.Screen.Default.RgbaVisual"/> should be used for creating windows with an alpha channel.
		///
		/// Get the system’s default visual for screen .
		/// This is the visual for the root window of the display.
		/// The return value should not be freed.
		/// </summary>
		public static Gdk.Visual SystemVisual => Gdk.Screen.Default.SystemVisual;

		public static double DefaultResolution => DefaultScreen.Resolution;

		/// <summary>
		/// GdkDisplay objects purpose are two fold:
		///	to manage and provide information about input devices (pointers and keyboards)
		///	to manage and provide information about the available <see cref="Gdk.Screen"/>s.
		///	<see cref="Gdk.Display"/>'s are the GDK representation of an X Display, which can be described as a workstation consisting of a keyboard,
		/// a pointing device (such as a mouse) and one or more screens.
		/// It is used to open and keep track of various <see cref="Gdk.Screen"/> objects currently instantiated by the application.
		/// It is also used to access the keyboard(s) and mouse pointer(s) of the display.
		/// </summary>
		public static Gdk.Display DefaultDisplay => Gdk.Display.Default;

		public static Gdk.Monitor CurrentMonitor => DefaultDisplay.GetMonitorAtPoint(0, 0); // TODO: find out aktual mouse position

		public static Gdk.Monitor PrimaryMonitor => GetMonitors().Single(m => m.IsPrimary);

		public static int CurrentScaleFaktor = CurrentMonitor.ScaleFactor;

		public static IEnumerable<Gdk.Monitor> GetMonitors() {

			for (var i = 0; i < DefaultDisplay.NMonitors; i++) {
				yield return DefaultDisplay.GetMonitor(i);
			}

		}

	}

}
