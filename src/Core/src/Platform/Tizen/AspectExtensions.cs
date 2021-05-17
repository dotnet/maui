using TAspect = Tizen.UIExtensions.Common.Aspect;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		public static TAspect ToNative(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => TAspect.AspectFit,
				Aspect.AspectFill => TAspect.AspectFill,
				Aspect.Fill => TAspect.Fill,
				_ => TAspect.AspectFit,
			};
	}
}