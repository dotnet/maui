using Gtk;

namespace Microsoft.Maui
{

	public static class TransformationExtensions
	{

		[MissingMapper]
		public static void UpdateScale(this Widget? platformView, double scale)
		{
			if (platformView == null || scale == 0)
				return;

			// fails: transform' is not a valid property name
			// https://mail.gnome.org/archives/gtk-list/2017-May/msg00015.html
			// nativeView.SetStyleValue($"scale({scale})", "transform");

		}

	}

}