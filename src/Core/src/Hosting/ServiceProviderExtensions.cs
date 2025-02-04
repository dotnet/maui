#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Maui
{
	internal static class ServiceProviderExtensions
	{
		internal static ILogger<T>? CreateLogger<T>(this IMauiContext context) =>
			context.Services.CreateLogger<T>();

		internal static ILogger CreateLogger(this IMauiContext context, Type type) =>
			context.Services.GetService<ILoggerFactory>()?.CreateLogger(type) ?? NullLogger.Instance;

		internal static ILogger<T>? CreateLogger<T>(this IServiceProvider services) =>
			services.GetService<ILogger<T>>();

		internal static ILogger? CreateLogger(this IMauiContext context, string loggerName) =>
			context.Services.CreateLogger(loggerName);

		internal static ILogger? CreateLogger(this IServiceProvider services, string loggerName) =>
			services.GetService<ILoggerFactory>()?.CreateLogger(loggerName);

		internal static ILogger CreateLogger(this IServiceProvider services, Type type) =>
			services.GetService<ILoggerFactory>()?.CreateLogger(type) ?? NullLogger.Instance;
	}
}