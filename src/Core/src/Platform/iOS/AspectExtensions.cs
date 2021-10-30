using UIKit;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		public static UIViewContentMode ToUIViewContentMode(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => UIViewContentMode.ScaleAspectFit,
				Aspect.AspectFill => UIViewContentMode.ScaleAspectFill,
				Aspect.Fill => UIViewContentMode.ScaleToFill,
				Aspect.Center => UIViewContentMode.Center,
				_ => UIViewContentMode.ScaleAspectFit,
			};
	}
}