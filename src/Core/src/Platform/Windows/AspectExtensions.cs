using WStretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		public static WStretch ToStretch(this Aspect aspect) =>
			aspect switch
			{
				Aspect.Fill => WStretch.Fill,
				Aspect.AspectFill => WStretch.UniformToFill,
				_ => WStretch.Uniform,
			};
	}
}