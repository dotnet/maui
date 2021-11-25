using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Samples {

	public class StartupTest {

		public static void InitTests() {

			var canvas = new PlatformCanvas();

			Logger.Info(FontExtensions.Default);
			
			using var desc = Pango.FontDescription.FromString(FontExtensions.Default.Family);
			Logger.Info(desc);

			var testStr = "123456";
			var size = canvas.GetStringSize(testStr, null, -1);
			Logger.Info($"{testStr} : {size}");

			size = canvas.GetStringSize(testStr, null, size.Width / 2);
			Logger.Info($"{testStr} : {size}");

			Logger.Info($"ScreenResulution {HardwareInformations.DefaultScreen.Resolution}");
			Logger.Info($"{nameof(HardwareInformations.CurrentScaleFaktor)} {HardwareInformations.CurrentScaleFaktor}");

		}

		void Notes() {
			// for context;
			var x = typeof(Gdk.CairoHelper);

			// for fonts:
			var y = typeof(Pango.CairoHelper);
		}

	}

}
