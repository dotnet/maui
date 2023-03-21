using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
		// see Android/HostBuilderExtension.cs for the real implementation
#if !ANDROID
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder) =>
			builder;
#endif
	}
}