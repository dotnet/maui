#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents a color and offset within a <see cref="GradientBrush"/>.</summary>
	public class GradientStop : Element
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			nameof(Color), typeof(Color), typeof(GradientStop), null);

		/// <summary>Gets or sets the color of this gradient stop. This is a bindable property.</summary>
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>Bindable property for <see cref="Offset"/>.</summary>
		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
			nameof(Offset), typeof(float), typeof(GradientStop), 0f);

		/// <summary>Gets or sets the position of this gradient stop (0.0 to 1.0). This is a bindable property.</summary>
		public float Offset
		{
			get => (float)GetValue(OffsetProperty);
			set => SetValue(OffsetProperty, value);
		}

		/// <summary>Initializes a new instance of the <see cref="GradientStop"/> class.</summary>
		public GradientStop() { }

		/// <summary>Initializes a new instance of the <see cref="GradientStop"/> class with the specified color and offset.</summary>
		/// <param name="color">The color of this gradient stop.</param>
		/// <param name="offset">The position of this gradient stop (0.0 to 1.0).</param>
		public GradientStop(Color color, float offset)
		{
			Color = color;
			Offset = offset;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is GradientStop dest))
				return false;

			return Color == dest.Color && global::System.Math.Abs(Offset - dest.Offset) < 0.00001;
		}

		/// <inheritdoc/>
		public override int GetHashCode() => base.GetHashCode();
	}
}