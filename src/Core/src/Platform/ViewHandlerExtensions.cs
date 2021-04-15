using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	internal static class ViewHandlerExtensions
	{
		public static T GetRequiredService<T>(this ViewHandler handler)
			where T : notnull
		{
			var context = handler.MauiContext ??
				throw new InvalidOperationException($"Unable to find the context. The {nameof(ViewHandler.MauiContext)} property should have been set by the host.");

			var services = context?.Services ??
				throw new InvalidOperationException($"Unable to find the service provider. The {nameof(ViewHandler.MauiContext)} property should have been set by the host.");

			var service = services.GetRequiredService<T>();

			return service;
		}
	}
}