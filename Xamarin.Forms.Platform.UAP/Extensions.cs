using System;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class Extensions
	{
		public static ConfiguredTaskAwaitable<T> DontSync<T>(this IAsyncOperation<T> self)
		{
			return self.AsTask().ConfigureAwait(false);
		}

		public static ConfiguredTaskAwaitable DontSync(this IAsyncAction self)
		{
			return self.AsTask().ConfigureAwait(false);
		}

		public static Windows.UI.Color GetIdealForegroundForBackgroundColor(this Windows.UI.Color backgroundColor)
		{
			var nThreshold = 105;
			int bgLuminance = Convert.ToInt32(backgroundColor.R * 0.2 + backgroundColor.G * 0.7 + backgroundColor.B * 0.1);

			Windows.UI.Color foregroundColor = 255 - bgLuminance < nThreshold ? Colors.Black : Colors.White;
			return foregroundColor;
		}

		public static void SetBinding(this FrameworkElement self, DependencyProperty property, string path)
		{
			self.SetBinding(property, new Windows.UI.Xaml.Data.Binding { Path = new PropertyPath(path) });
		}

		public static void SetBinding(this FrameworkElement self, DependencyProperty property, string path, Windows.UI.Xaml.Data.IValueConverter converter)
		{
			self.SetBinding(property, new Windows.UI.Xaml.Data.Binding { Path = new PropertyPath(path), Converter = converter });
		}
	}
}