#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public static class ServiceProviderExtensions
	{
		internal static ILogger<T>? CreateLogger<T>(this IServiceProvider services) =>
			services.GetService<ILoggerFactory>()?.CreateLogger<T>();
	}
}