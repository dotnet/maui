// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Services;
using Moq;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

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
	public void CreateTestServiceProvider_UsesProvidedMocks()
	{
		// Arrange
		var mockAndroid = new Mock<IAndroidProvider>();
		var mockApple = new Mock<IAppleProvider>();

		// Act
		var provider = ServiceConfiguration.CreateTestServiceProvider(
			androidProvider: mockAndroid.Object,
			appleProvider: mockApple.Object);

		// Assert
		Assert.Same(mockAndroid.Object, provider.GetService<IAndroidProvider>());
		Assert.Same(mockApple.Object, provider.GetService<IAppleProvider>());
	}

	[Fact]
	public void CreateTestServiceProvider_CreatesMissingServices()
	{
		// Arrange - only provide android mock
		var mockAndroid = new Mock<IAndroidProvider>();

		// Act
		var provider = ServiceConfiguration.CreateTestServiceProvider(
			androidProvider: mockAndroid.Object);

		// Assert - should create real apple provider
		Assert.Same(mockAndroid.Object, provider.GetService<IAndroidProvider>());
		Assert.NotNull(provider.GetService<IAppleProvider>());
		Assert.IsType<AppleProvider>(provider.GetService<IAppleProvider>());
	}

	[Fact]
	public void Program_Services_CanBeOverridden()
	{
		try
		{
			// Arrange
			var mockAndroid = new Mock<IAndroidProvider>();
			var testProvider = ServiceConfiguration.CreateTestServiceProvider(
				androidProvider: mockAndroid.Object);

			// Act
			Program.Services = testProvider;

			// Assert
			Assert.Same(mockAndroid.Object, Program.AndroidProvider);
		}
		finally
		{
			// Cleanup
			Program.ResetServices();
		}
	}
}
