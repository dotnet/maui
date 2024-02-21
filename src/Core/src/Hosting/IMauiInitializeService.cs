using System;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// Represents a service that is initialized during the application construction.
	/// </summary>
	/// <remarks>
	/// This service is initialized during the MauiAppBuilder.Build() method. It is
	/// executed once per application using the root service provider.
	/// </remarks>
	public interface IMauiInitializeService
	{
		void Initialize(IServiceProvider services);
	}

	/// <summary>
	/// Represents a service that is initialized during the window construction.
	/// </summary>
	/// <remarks>
	/// This service is initialized during the creation of a window. It is
	/// executed once per window using the window-scoped service provider.
	/// </remarks>
	public interface IMauiInitializeScopedService
	{
		void Initialize(IServiceProvider services);
	}
}