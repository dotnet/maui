using Xamarin.Forms.Platform;

namespace Xamarin.Forms.Shapes
{
	[RenderWith(typeof(_EllipseRenderer))]
	public sealed class Ellipse : Shape
	{
		public Ellipse()
		{
			Aspect = Stretch.Fill;
		}
	}
}