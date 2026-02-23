// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Services;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools;

/// <summary>
/// Configures dependency injection for the application.
/// </summary>
public static class ServiceConfiguration
{
	/// <summary>
	/// Creates and configures the service provider.
	/// </summary>
	public static IServiceProvider CreateServiceProvider()
	{
		var services = new ServiceCollection();
		ConfigureServices(services);
		return services.BuildServiceProvider();
	}

	/// <summary>
	/// Configures services for dependency injection.
	/// </summary>
	public static void ConfigureServices(IServiceCollection services)
	{
		// Android providers
		services.AddSingleton<IJdkManager, JdkManager>();
		services.AddSingleton<IAndroidProvider, AndroidProvider>();

		// Apple providers (only functional on macOS, but registered everywhere)
		services.AddSingleton<IAppleProvider, AppleProvider>();

		// Core services
		services.AddSingleton<IDoctorService, DoctorService>();
		services.AddSingleton<IDeviceManager, DeviceManager>();

		// Output formatters (transient - created per request with specific config)
		services.AddTransient<JsonOutputFormatter>();
		services.AddTransient<ConsoleOutputFormatter>();
	}

	/// <summary>
	/// Creates services for testing with custom implementations.
	/// </summary>
	public static IServiceProvider CreateTestServiceProvider(
		IAndroidProvider? androidProvider = null,
		IAppleProvider? appleProvider = null,
		IJdkManager? jdkManager = null,
		IDoctorService? doctorService = null,
		IDeviceManager? deviceManager = null)
	{
		var services = new ServiceCollection();

		// Use provided mocks or create real implementations
		if (jdkManager != null)
			services.AddSingleton(jdkManager);
		else
			services.AddSingleton<IJdkManager, JdkManager>();

		if (androidProvider != null)
			services.AddSingleton(androidProvider);
		else
			services.AddSingleton<IAndroidProvider, AndroidProvider>();

		if (appleProvider != null)
			services.AddSingleton(appleProvider);
		else
			services.AddSingleton<IAppleProvider, AppleProvider>();

		if (doctorService != null)
			services.AddSingleton(doctorService);
		else
			services.AddSingleton<IDoctorService, DoctorService>();

		if (deviceManager != null)
			services.AddSingleton(deviceManager);
		else
			services.AddSingleton<IDeviceManager, DeviceManager>();

		return services.BuildServiceProvider();
	}
}
