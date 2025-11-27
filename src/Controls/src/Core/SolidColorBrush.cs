#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="Type[@FullName='Microsoft.Maui.Controls.SolidColorBrush']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
	[ContentProperty(nameof(Color))]
	public class SolidColorBrush : Brush
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public SolidColorBrush()
		{

		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public SolidColorBrush(Color color)
		{
			Color = color;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public virtual Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <param name="obj">The obj parameter.</param>
		public override bool Equals(object obj)
		{
			if (!(obj is SolidColorBrush dest))
				return false;

			return Color == dest.Color;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SolidColorBrush.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode() => base.GetHashCode();
	}
}