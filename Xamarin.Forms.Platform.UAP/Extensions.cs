using System;
using System.Runtime.CompilerServices;
using Windows.Foundation;
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