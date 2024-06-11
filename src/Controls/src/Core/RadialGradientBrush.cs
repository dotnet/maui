#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadialGradientBrush']/Docs/*" />
	public class RadialGradientBrush : GradientBrush
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public RadialGradientBrush()
		{

		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public RadialGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public RadialGradientBrush(GradientStopCollection gradientStops, double radius)
		{
			GradientStops = gradientStops;
			Radius = radius;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
		public RadialGradientBrush(GradientStopCollection gradientStops, Point center, double radius)
		{
			GradientStops = gradientStops;
			Center = center;
			Radius = radius;
		}

		public override bool IsEmpty => base.IsEmpty;

		/// <summary>Bindable property for <see cref="Center"/>.</summary>
		public static readonly BindableProperty CenterProperty = BindableProperty.Create(
			nameof(Center), typeof(Point), typeof(RadialGradientBrush), new Point(0.5, 0.5));

		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='Center']/Docs/*" />
		public Point Center
		{
			get => (Point)GetValue(CenterProperty);
			set => SetValue(CenterProperty, value);
		}

		/// <summary>Bindable property for <see cref="Radius"/>.</summary>
		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
			nameof(Radius), typeof(double), typeof(RadialGradientBrush), 0.5d);

		/// <include file="../../docs/Microsoft.Maui.Controls/RadialGradientBrush.xml" path="//Member[@MemberName='Radius']/Docs/*" />
		public double Radius
		{
			get => (double)GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}
	}
}