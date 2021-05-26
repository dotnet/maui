using System;
using Windows.UI;
using Microsoft.UI;
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class ColorExtensions
	{
		[Obsolete("ToBrush is obsolete. Please use ToNative instead")]
		public static WBrush ToBrush(this Graphics.Color color) => color.ToNative();


		[Obsolete("ToFormsColor is obsolete. Please use ToColor instead")]
		public static Graphics.Color ToFormsColor(this Windows.UI.Color color)
		{
			return color.ToColor();
		}

		[Obsolete("ToFormsColor is obsolete. Please use ToColor instead")]
		public static Graphics.Color ToFormsColor(this WSolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToColor();
		}
	}
}
