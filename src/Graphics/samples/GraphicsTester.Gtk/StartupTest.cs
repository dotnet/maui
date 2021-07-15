using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Samples {

	public class StartupTest {

		public static void InitTests() {

			Logger.Info(NativeFontService.Instance.SystemFontDescription);
			Logger.Info(NativeFontService.Instance.SystemFontName);
			Logger.Info(NativeFontService.Instance.BoldSystemFontName);

			foreach (var ff in NativeFontService.Instance.GetFontFamilies()) {
				Logger.Info(ff);

				foreach (var s in ff.GetFontStyles()) {
					Logger.Info($"\t\t{s}");

				}
			}

			using var desc = Pango.FontDescription.FromString(NativeFontService.Instance.SystemFontName);
			Logger.Info(desc);

			var testStr = "123456";
			var size = NativeGraphicsService.Instance.GetStringSize(testStr, null, -1);
			Logger.Info($"{testStr} : {size}");

			size = NativeGraphicsService.Instance.GetStringSize(testStr, null, size.Width / 2);
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
