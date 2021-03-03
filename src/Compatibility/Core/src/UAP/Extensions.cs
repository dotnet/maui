using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using Microsoft.UI.Xaml.Media.Imaging;
//using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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
			self.SetBinding(property, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath(path) });
		}

		public static void SetBinding(this FrameworkElement self, DependencyProperty property, string path, Microsoft.UI.Xaml.Data.IValueConverter converter)
		{
			self.SetBinding(property, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath(path), Converter = converter });
		}

		internal static InputScopeNameValue GetKeyboardButtonType(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Default:
				case ReturnType.Done:
				case ReturnType.Go:
				case ReturnType.Next:
				case ReturnType.Send:
					return InputScopeNameValue.Default;
				case ReturnType.Search:
					return InputScopeNameValue.Search;
				default:
					throw new NotImplementedException($"ReturnType {returnType} not supported");
			}
		}

		internal static InputScope ToInputScope(this ReturnType returnType)
		{
			var scopeName = new InputScopeName()
			{
				NameValue = GetKeyboardButtonType(returnType)
			};

			var inputScope = new InputScope
			{
				Names = { scopeName }
			};

			return inputScope;
		}

		internal static UwpScrollBarVisibility ToUwpScrollBarVisibility(this ScrollBarVisibility visibility)
		{
			switch (visibility)
			{
				case ScrollBarVisibility.Always:
					return UwpScrollBarVisibility.Visible;
				case ScrollBarVisibility.Default:
					return UwpScrollBarVisibility.Auto;
				case ScrollBarVisibility.Never:
					return UwpScrollBarVisibility.Hidden;
				default:
					return UwpScrollBarVisibility.Auto;
			}
		}

		public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) < 0)
				return min;
			if (value.CompareTo(max) > 0)
				return max;
			return value;
		}


		internal static int ToEm(this double pt)
		{
			return Convert.ToInt32( pt * 0.0624f * 1000); //Coefficient for converting Pt to Em. The value is uniform spacing between characters, in units of 1/1000 of an em.
		}
	}
}