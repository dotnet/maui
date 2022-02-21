using TAspect = Tizen.UIExtensions.Common.Aspect;

namespace Microsoft.Maui.Platform
{
	public static class AspectExtensions
	{
		public static TAspect ToPlatform(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => TAspect.AspectFit,
				Aspect.AspectFill => TAspect.AspectFill,
				Aspect.Fill => TAspect.Fill,
				_ => TAspect.AspectFit,
			};
	}
}