using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Foldable
{
	/// <summary>
	/// Extension methods for configuring foldable device support in .NET MAUI applications.
	/// </summary>
	public static partial class HostBuilderExtensions
	{
		// Platform implementations register their service in platform-specific files.
#if !ANDROID && !IOS_DUO_BINDINGS
		/// <summary>
		/// Configures the app to detect and respond to foldable device hinge positions and screen configurations.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder) =>
			builder;
#endif
	}
}