// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Providers.Apple;
using Microsoft.Maui.Client.Services;
using Microsoft.Maui.Client.Tests.Fakes;
using Xunit;

namespace Microsoft.Maui.Client.Tests;

public class ServiceConfigurationTests
{
	[Fact]
	public void CreateServiceProvider_RegistersAllServices()
	{
		// Act
		var provider = ServiceConfiguration.CreateServiceProvider();

		// Assert
		Assert.NotNull(provider.GetService<IJdkManager>());
		Assert.NotNull(provider.GetService<IAndroidProvider>());
		Assert.NotNull(provider.GetService<IAppleProvider>());
		Assert.NotNull(provider.GetService<IDoctorService>());
		Assert.NotNull(provider.GetService<IDeviceManager>());
	}

	[Fact]
	public void CreateServiceProvider_ReturnsSingletonForProviders()
	{
		// Act
		var provider = ServiceConfiguration.CreateServiceProvider();
		var android1 = provider.GetService<IAndroidProvider>();
		var android2 = provider.GetService<IAndroidProvider>();

		// Assert
		Assert.Same(android1, android2);
	}

	[Fact]
	public void CreateTestServiceProvider_UsesProvidedFakes()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider();
		var fakeApple = new FakeAppleProvider();

		// Act
		var provider = ServiceConfiguration.CreateTestServiceProvider(
			androidProvider: fakeAndroid,
			appleProvider: fakeApple);

		// Assert
		Assert.Same(fakeAndroid, provider.GetService<IAndroidProvider>());
		Assert.Same(fakeApple, provider.GetService<IAppleProvider>());
	}

	[Fact]
	public void CreateTestServiceProvider_CreatesMissingServices()
	{
		// Arrange - only provide android fake
		var fakeAndroid = new FakeAndroidProvider();

		// Act
		var provider = ServiceConfiguration.CreateTestServiceProvider(
			androidProvider: fakeAndroid);

		// Assert - should create real apple provider
		Assert.Same(fakeAndroid, provider.GetService<IAndroidProvider>());
		Assert.NotNull(provider.GetService<IAppleProvider>());
		Assert.IsType<AppleProvider>(provider.GetService<IAppleProvider>());
	}

	[Fact]
	public void Program_Services_CanBeOverridden()
	{
		try
		{
			// Arrange
			var fakeAndroid = new FakeAndroidProvider();
			var testProvider = ServiceConfiguration.CreateTestServiceProvider(
				androidProvider: fakeAndroid);

			// Act
			Program.Services = testProvider;

			// Assert
			Assert.Same(fakeAndroid, Program.AndroidProvider);
		}
		finally
		{
			// Cleanup
			Program.ResetServices();
		}
	}
}
