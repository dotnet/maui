using Xamarin.Forms.Platform;

namespace Xamarin.Forms.Shapes
{
	[RenderWith(typeof(_RectangleRenderer))]
	public sealed class Rectangle : Shape
	{
		public Rectangle()
		{
			Aspect = Stretch.Fill;
		}

		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(Rectangle), 0.0d);

		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(Rectangle), 0.0d);

		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}
	}
}