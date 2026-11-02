using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Represents an element which is visually drawn on the <see cref="Map"/> control.
	/// </summary>
	public partial class MapElement : Element
	{
		/// <summary>Bindable property for <see cref="StrokeColor"/>.</summary>
		public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
			nameof(StrokeColor),
			typeof(Color),
			typeof(MapElement),
			null);

		/// <summary>Bindable property for <see cref="StrokeWidth"/>.</summary>
		public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
			nameof(StrokeWidth),
			typeof(float),
			typeof(MapElement),
			5f);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
			nameof(IsVisible),
			typeof(bool),
			typeof(MapElement),
			true);

		/// <summary>Bindable property for <see cref="ZIndex"/>.</summary>
		public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(
			nameof(ZIndex),
			typeof(int),
			typeof(MapElement),
			0);

		/// <summary>
		/// Gets or sets the stroke color. This is a bindable property.
		/// </summary>
		public Color StrokeColor
		{
			get => (Color)GetValue(StrokeColorProperty);
			set => SetValue(StrokeColorProperty, value);
		}

		/// <summary>
		/// Gets or sets the stroke width. The default value is <c>5f</c>.
		/// This is a bindable property.
		/// </summary>
		public float StrokeWidth
		{
			get => (float)GetValue(StrokeWidthProperty);
			set => SetValue(StrokeWidthProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the map element is visible on the map.
		/// The default value is <see langword="true"/>.
		/// This is a bindable property.
		/// </summary>
		public bool IsVisible
		{
			get => (bool)GetValue(IsVisibleProperty);
			set => SetValue(IsVisibleProperty, value);
		}

		/// <summary>
		/// Gets or sets the z-index of the map element, which controls its draw order relative to other elements.
		/// Higher values are drawn on top of lower values. The default value is <c>0</c>.
		/// This is a bindable property.
		/// </summary>
		public int ZIndex
		{
			get => (int)GetValue(ZIndexProperty);
			set => SetValue(ZIndexProperty, value);
		}

		/// <summary>
		/// Gets or sets the platform counterpart of this map element.
		/// </summary>
		/// <remarks>This should typically not be set by the developer. Doing so might result in unpredictable behavior.</remarks>
		public object? MapElementId { get; set; }
	}
}
