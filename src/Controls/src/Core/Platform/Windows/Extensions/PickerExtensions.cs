using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateHorizontalOptions(this FrameworkElement platformView, View view)
		{
			platformView.HorizontalAlignment = view.HorizontalOptions.Alignment switch
			{
				LayoutAlignment.Start => HorizontalAlignment.Left,
				LayoutAlignment.Center => HorizontalAlignment.Center,
				LayoutAlignment.End => HorizontalAlignment.Right,
				LayoutAlignment.Fill => HorizontalAlignment.Stretch,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public static void UpdateVerticalOptions(this FrameworkElement platformView, View view)
		{
			platformView.VerticalAlignment = view.VerticalOptions.Alignment switch
			{
				LayoutAlignment.Start => VerticalAlignment.Top,
				LayoutAlignment.Center => VerticalAlignment.Center,
				LayoutAlignment.End => VerticalAlignment.Bottom,
				LayoutAlignment.Fill => VerticalAlignment.Stretch,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
