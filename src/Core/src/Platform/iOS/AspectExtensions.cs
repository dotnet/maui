using UIKit;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		public static UIViewContentMode ToUIViewContentMode(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFill => UIViewContentMode.ScaleAspectFill,
				Aspect.Fill => UIViewContentMode.ScaleToFill,
				_ => UIViewContentMode.ScaleAspectFit,
			};
	}
}