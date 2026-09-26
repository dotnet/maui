#if IOS_DUO_BINDINGS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
		/// <summary>
		/// Configures the app to detect and respond to foldable device hinge positions and screen configurations.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder)
		{
			builder.Services.AddScoped<IFoldableService, FoldableService>();
			return builder;
		}
	}
}
#endif
