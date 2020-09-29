using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Xamarin.Forms.Maps
{
	public class MapElement : Element
	{
		public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
			nameof(StrokeColor),
			typeof(Color),
			typeof(MapElement),
			Color.Default);

		public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
			nameof(StrokeWidth),
			typeof(float),
			typeof(MapElement),
			5f);

		public Color StrokeColor
		{
			get => (Color)GetValue(StrokeColorProperty);
			set => SetValue(StrokeColorProperty, value);
		}

		public float StrokeWidth
		{
			get => (float)GetValue(StrokeWidthProperty);
			set => SetValue(StrokeWidthProperty, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public object MapElementId { get; set; }
	}
}