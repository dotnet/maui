#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class BorderElement
	{
		public const int DefaultCornerRadius = -1;

		/// <summary>Bindable property for <see cref="IBorderElement.BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create(nameof(IBorderElement.BorderColor), typeof(Color), typeof(IBorderElement), null,
									propertyChanged: OnBorderColorPropertyChanged);

		/// <summary>Bindable property for <see cref="IBorderElement.BorderWidth"/>.</summary>
		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(IBorderElement.BorderWidth), typeof(double), typeof(IBorderElement), -1d);

		/// <summary>Bindable property for <see cref="IBorderElement.CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(IBorderElement.CornerRadius), typeof(int), typeof(IBorderElement), defaultValue: DefaultCornerRadius);

		static void OnBorderColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IBorderElement)bindable).OnBorderColorPropertyChanged((Color)oldValue, (Color)newValue);
		}
	}
}