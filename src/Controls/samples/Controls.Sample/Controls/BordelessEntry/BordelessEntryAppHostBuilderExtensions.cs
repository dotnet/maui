using System;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	static class BordelessEntryAppHostBuilderExtensions
	{
		public static IAppHostBuilder UseBordelessEntry(this IAppHostBuilder builder, Action<BordelessEntryServiceBuilder> configureDelegate = null)
		{
			builder.ConfigureServices<BordelessEntryServiceBuilder>((ctx, red) => configureDelegate?.Invoke(red));

			return builder;
		}
	}
}
