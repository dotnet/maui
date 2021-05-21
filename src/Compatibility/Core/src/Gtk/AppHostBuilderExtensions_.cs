using Microsoft.Maui.Handlers.ScrollView;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{

	public static class AppHostBuilderExtensions_
	{

		public static IAppHostBuilder UseCompatibilityRenderers(this IAppHostBuilder builder)
		{

			builder.ConfigureMauiHandlers(col =>
			{
				col.AddHandler<ScrollView,ScrollViewHandler>();
			});
			return builder;
		}
		
		public static IAppHostBuilder UseFormsCompatibility(this IAppHostBuilder builder)
		{

			return builder;
		}
		

	}

}
