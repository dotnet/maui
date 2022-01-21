using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Samples {

	public class StartupTest {

		public static void InitTests() {

			var canvas = new PlatformCanvas();

			Console.WriteLine(FontExtensions.Default);
			
			using var desc = Pango.FontDescription.FromString(FontExtensions.Default.Family);
			Console.WriteLine(desc);

			var testStr = "123456";
			var size = canvas.GetStringSize(testStr, null, -1);
			Console.WriteLine($"{testStr} : {size}");

			size = canvas.GetStringSize(testStr, null, size.Width / 2);
			Console.WriteLine($"{testStr} : {size}");

			Console.WriteLine($"ScreenResulution {HardwareInformations.DefaultScreen.Resolution}");
			Console.WriteLine($"{nameof(HardwareInformations.CurrentScaleFaktor)} {HardwareInformations.CurrentScaleFaktor}");

		}

		void Notes() {
			// for context;
			var x = typeof(Gdk.CairoHelper);

			// for fonts:
			var y = typeof(Pango.CairoHelper);
		}

	}

}
