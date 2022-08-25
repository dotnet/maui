using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class MapElement : Element
	{
		public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
			nameof(StrokeColor),
			typeof(Color),
			typeof(MapElement),
			null);

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

		public object? MapElementId { get; set; }
	}
}
