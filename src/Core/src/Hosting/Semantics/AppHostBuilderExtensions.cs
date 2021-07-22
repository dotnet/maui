using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureSemantics(this IAppHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				services.AddSingleton<ISemanticService>(svcs => new SemanticService());
			});

			return builder;
		}
	}
}