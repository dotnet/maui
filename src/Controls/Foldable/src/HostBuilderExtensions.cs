using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
#if !ANDROID
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder) =>
			builder;
#endif
	}
}