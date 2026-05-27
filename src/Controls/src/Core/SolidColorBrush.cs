#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Brush"/> that paints an area with a single solid color.</summary>
	[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
	[ContentProperty(nameof(Color))]
	public class SolidColorBrush : Brush
	{
		/// <summary>Initializes a new instance of the <see cref="SolidColorBrush"/> class.</summary>
		public SolidColorBrush()
		{

		}

		/// <summary>Initializes a new instance of the <see cref="SolidColorBrush"/> class with the specified color.</summary>
		/// <param name="color">The color of the brush.</param>
		public SolidColorBrush(Color color)
		{
			Color = color;
		}

		/// <summary>Gets a value indicating whether this brush is empty.</summary>
		public override bool IsEmpty
		{
			get
			{
				var solidColorBrush = this;
				return solidColorBrush == null || solidColorBrush.Color == null;
			}
		}

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			nameof(Color), typeof(Color), typeof(SolidColorBrush), null);

		/// <summary>Gets or sets the color of this brush. This is a bindable property.</summary>
		public virtual Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>Determines whether the specified object is equal to the current <see cref="SolidColorBrush"/>.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is SolidColorBrush dest))
				return false;

			return Equals(Color, dest.Color);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => Color?.GetHashCode() ?? 0;
	}
}