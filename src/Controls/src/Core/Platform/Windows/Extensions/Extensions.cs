using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.UI.Text;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
//using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
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
	}
}