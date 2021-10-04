using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Maui.Hosting
{
	static class MauiAppBuilderExtensions
	{
		public static MauiAppBuilder ConfigureCoreServices(this MauiAppBuilder builder)
		{
			builder.Services.TryAddSingleton<IHashAlgorithm, Crc64>();
			return builder;
		}
	}
}