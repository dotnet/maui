using System;

namespace Xamarin.Forms.Platform.iOS
{
	public static class BarAppearanceTrackerUtils
	{
		public static Color UnblendColor(Color color)
		{
			// iOS is going to ignore any alpha mask on this color so we need
			// to just assume its a solid color. We'll "blend" it with white.

			var r = color.R;
			var g = color.G;
			var b = color.B;
			var a = color.A;

			r = (r * a) + (1 - a);
			g = (g * a) + (1 - a);
			b = (b * a) + (1 - a);

			// Now iOS is going to blend with white at 86% opacity, so we need to "darken" the
			// color we pass in order to compensate

			r = UnblendBackgroundChannel(r, .863);
			g = UnblendBackgroundChannel(g, .863);
			b = UnblendBackgroundChannel(b, .863);

			return new Color(r, g, b);
		}

		static double UnblendBackgroundChannel(double channel, double alpha)
		{
			// basically iOS is going to blend us with white to produce its final color,
			// so we are going to apply the default blend in reverse to produce a true color.

			// Unfortunately "unblending" only works for a subset of all possible values as you
			// can get a negative number. This means that there is no source input value
			// for which the requested color can be retreived.

			// In this case we just clamp to 0
			return Math.Max(0, (channel - (1 - alpha)) / alpha);
		}
	}
}