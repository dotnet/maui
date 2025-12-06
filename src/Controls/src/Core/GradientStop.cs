#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="Type[@FullName='Microsoft.Maui.Controls.GradientStop']/Docs/*" />
	public class GradientStop : Element
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(
			nameof(Color), typeof(Color), typeof(GradientStop), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>Bindable property for <see cref="Offset"/>.</summary>
		public static readonly BindableProperty OffsetProperty = BindableProperty.Create(
			nameof(Offset), typeof(float), typeof(GradientStop), 0f);

		/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="//Member[@MemberName='Offset']/Docs/*" />
		public float Offset
		{
			get => (float)GetValue(OffsetProperty);
			set => SetValue(OffsetProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public GradientStop() { }

		/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public GradientStop(Color color, float offset)
		{
			Color = color;
			Offset = offset;
		}

		/// <param name="obj">The obj parameter.</param>
		public override bool Equals(object obj)
		{
			if (!(obj is GradientStop dest))
				return false;

			return Color == dest.Color && global::System.Math.Abs(Offset - dest.Offset) < 0.00001;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/GradientStop.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode() => base.GetHashCode();
	}
}