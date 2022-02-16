using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class MenuFlyoutSubItem : Menu<IMenuFlyoutItemBase>, IMenuFlyoutSubItem
	{
		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(MenuFlyoutSubItem), default(ImageSource));

		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuFlyoutSubItem), null);

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public ImageSource Icon
		{
			get => (ImageSource)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		IImageSource IImageSourcePart.Source => this.Icon;

		bool IImageSourcePart.IsAnimationPlaying => false;

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}

	}
}