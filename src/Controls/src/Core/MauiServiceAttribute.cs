using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MauiServiceAttribute : Attribute
	{
		/// <summary>
		/// Type gets registered in the DI container, defaults to Singleton
		/// </summary>
		public MauiServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton) => Lifetime = lifetime;

		/// <summary>
		/// Lifetime of the Service type on which this attribute is defined.
		/// </summary>
		public ServiceLifetime Lifetime { get; }

		/// <summary>
		/// The type for which this service would be resolved for.
		/// <code>builder.Services.AddSingleton&lt;INavigationService, NavigationService&gt;();</code>
		/// </summary>
		public Type? RegisterFor { get; set; }

		/// <summary>
		/// If set to true, makes use of the TryAdd method construct while registering the service.
		/// </summary>
		public bool UseTryAdd { get; set; }
	}
}
