using System;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	static class RedButtonAppHostBuilderExtensions
	{
		public static IAppHostBuilder UseRed(this IAppHostBuilder builder, Action<RedServiceBuilder> configureDelegate = null)
		{
			builder.ConfigureServices<RedServiceBuilder>((ctx, red) => configureDelegate?.Invoke(red));

			return builder;
		}
	}
}