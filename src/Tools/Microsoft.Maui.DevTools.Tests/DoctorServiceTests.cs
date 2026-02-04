// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Services;
using Moq;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class DoctorServiceTests
{
	[Fact]
	public async Task RunAllChecksAsync_IncludesDotNetChecks()
	{
		// Arrange
		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var service = new DoctorService(mockAndroid.Object, mockApple.Object);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert
		Assert.NotNull(report);
		Assert.True(report.Checks.Any(c => c.Category == "dotnet"));
	}

	[Fact]
	public async Task RunAllChecksAsync_IncludesAndroidChecks_WhenProviderReturnsChecks()
	{
		// Arrange
		var androidChecks = new List<HealthCheck>
		{
			new HealthCheck
			{
				Category = "android",
				Name = "JDK",
				Status = CheckStatus.Ok,
				Message = "JDK 17"
			},
			new HealthCheck
			{
				Category = "android",
				Name = "Android SDK",
				Status = CheckStatus.Ok
			}
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(androidChecks);

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var service = new DoctorService(mockAndroid.Object, mockApple.Object);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert
		Assert.Contains(report.Checks, c => c.Category == "android" && c.Name == "JDK");
		Assert.Contains(report.Checks, c => c.Category == "android" && c.Name == "Android SDK");
	}

	[Fact]
	public async Task RunAllChecksAsync_CalculatesCorrectSummary()
	{
		// Arrange
		var androidChecks = new List<HealthCheck>
		{
			new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok },
			new HealthCheck { Category = "android", Name = "SDK", Status = CheckStatus.Warning },
			new HealthCheck { Category = "android", Name = "AVD", Status = CheckStatus.Error }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(androidChecks);

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var service = new DoctorService(mockAndroid.Object, mockApple.Object);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert - should have dotnet checks + android checks
		Assert.True(report.Summary.Total >= 3); // At least 3 checks
		Assert.True(report.Summary.Warning >= 1); // At least 1 warning
		Assert.True(report.Summary.Error >= 1); // At least 1 error
	}

	[Fact]
	public async Task RunAllChecksAsync_SetsStatusBasedOnChecks()
	{
		// Arrange - all OK
		var okChecks = new List<HealthCheck>
		{
			new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(okChecks);

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var service = new DoctorService(mockAndroid.Object, mockApple.Object);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert - status should reflect worst check
		Assert.NotEqual(HealthStatus.Unhealthy, report.Status);
	}

	[Fact]
	public async Task RunAllChecksAsync_RespectsCategory_WhenSpecified()
	{
		// Arrange
		var androidChecks = new List<HealthCheck>
		{
			new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(androidChecks);

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.CheckHealthAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<HealthCheck>());

		var service = new DoctorService(mockAndroid.Object, mockApple.Object);

		// Act
		var report = await service.RunChecksByCategoryAsync("android");

		// Assert
		Assert.All(report.Checks, c => Assert.Equal("android", c.Category));
	}
}
