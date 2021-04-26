using WStretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		public static WStretch ToStretch(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => WStretch.Uniform,
				Aspect.AspectFill => WStretch.UniformToFill,
				Aspect.Fill => WStretch.Fill,
				Aspect.Center => WStretch.None,
				_ => WStretch.Uniform,
			};
	}
}