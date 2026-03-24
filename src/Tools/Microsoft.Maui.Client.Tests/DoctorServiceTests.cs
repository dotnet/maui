// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Services;
using Microsoft.Maui.Client.Tests.Fakes;
using Xunit;

namespace Microsoft.Maui.Client.Tests;

public class DoctorServiceTests
{
	[Fact]
	public async Task RunAllChecksAsync_IncludesDotNetChecks()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider();
		var fakeApple = new FakeAppleProvider();
		var service = new DoctorService(fakeAndroid, fakeApple);

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
		var fakeAndroid = new FakeAndroidProvider
		{
			HealthChecks = new List<HealthCheck>
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
			}
		};

		var fakeApple = new FakeAppleProvider();
		var service = new DoctorService(fakeAndroid, fakeApple);

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
		var fakeAndroid = new FakeAndroidProvider
		{
			HealthChecks = new List<HealthCheck>
			{
				new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok },
				new HealthCheck { Category = "android", Name = "SDK", Status = CheckStatus.Warning },
				new HealthCheck { Category = "android", Name = "AVD", Status = CheckStatus.Error }
			}
		};

		var fakeApple = new FakeAppleProvider();
		var service = new DoctorService(fakeAndroid, fakeApple);

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
		var fakeAndroid = new FakeAndroidProvider
		{
			HealthChecks = new List<HealthCheck>
			{
				new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok }
			}
		};

		var fakeApple = new FakeAppleProvider();
		var service = new DoctorService(fakeAndroid, fakeApple);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert - status should reflect worst check
		Assert.NotEqual(HealthStatus.Unhealthy, report.Status);
	}

	[Fact]
	public async Task RunAllChecksAsync_IncludesAndroidChecks_WhenProviderReturnsAndroidOnly()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider
		{
			HealthChecks = new List<HealthCheck>
			{
				new HealthCheck { Category = "android", Name = "JDK", Status = CheckStatus.Ok }
			}
		};

		var fakeApple = new FakeAppleProvider();
		var service = new DoctorService(fakeAndroid, fakeApple);

		// Act
		var report = await service.RunAllChecksAsync();

		// Assert - android checks should be present
		Assert.Contains(report.Checks, c => c.Category == "android" && c.Name == "JDK");
	}
}
