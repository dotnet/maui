#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Rectangle']/Docs/*" />
	public sealed partial class Rectangle : Shape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Rectangle() : base()
		{
			Aspect = Stretch.Fill;
		}

		/// <summary>Bindable property for <see cref="RadiusX"/>.</summary>
		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(Rectangle), 0.0d);

		/// <summary>Bindable property for <see cref="RadiusY"/>.</summary>
		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(Rectangle), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='RadiusX']/Docs/*" />
		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='RadiusY']/Docs/*" />
		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}
	}
}