// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Providers.Android;
using Moq;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class AndroidProviderTests
{
	[Fact]
	public async Task GetMostRecentSystemImageAsync_ReturnsHighestApiLevel()
	{
		// Arrange
		var packages = new List<SdkPackage>
		{
			new SdkPackage { Path = "system-images;android-33;google_apis;arm64-v8a" },
			new SdkPackage { Path = "system-images;android-35;google_apis;arm64-v8a" },
			new SdkPackage { Path = "system-images;android-34;google_apis;arm64-v8a" },
			new SdkPackage { Path = "platform-tools" }, // Not a system image
			new SdkPackage { Path = "build-tools;34.0.0" } // Not a system image
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetInstalledPackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(packages);
		mockProvider.Setup(x => x.GetMostRecentSystemImageAsync(It.IsAny<CancellationToken>()))
			.Returns(async (CancellationToken ct) =>
			{
				var pkgs = await mockProvider.Object.GetInstalledPackagesAsync(ct);
				return pkgs
					.Where(p => p.Path.StartsWith("system-images;android-", StringComparison.OrdinalIgnoreCase))
					.Select(p => new { Package = p, ApiLevel = ExtractApiLevel(p.Path) })
					.Where(x => x.ApiLevel > 0)
					.OrderByDescending(x => x.ApiLevel)
					.FirstOrDefault()?.Package.Path;
			});

		// Act
		var result = await mockProvider.Object.GetMostRecentSystemImageAsync();

		// Assert
		Assert.Equal("system-images;android-35;google_apis;arm64-v8a", result);
	}

	[Fact]
	public async Task GetMostRecentSystemImageAsync_ReturnsNull_WhenNoSystemImages()
	{
		// Arrange
		var packages = new List<SdkPackage>
		{
			new SdkPackage { Path = "platform-tools" },
			new SdkPackage { Path = "build-tools;34.0.0" }
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetInstalledPackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(packages);
		mockProvider.Setup(x => x.GetMostRecentSystemImageAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync((string?)null);

		// Act
		var result = await mockProvider.Object.GetMostRecentSystemImageAsync();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task CreateAvdAsync_CreatesAvdWithCorrectParameters()
	{
		// Arrange
		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.CreateAvdAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((string name, string device, string image, bool force, CancellationToken ct) =>
				new AvdInfo { Name = name, DeviceProfile = device, SystemImage = image });

		// Act
		var result = await mockProvider.Object.CreateAvdAsync(
			"TestEmulator",
			"pixel_6",
			"system-images;android-35;google_apis;arm64-v8a");

		// Assert
		Assert.Equal("TestEmulator", result.Name);
		Assert.Equal("pixel_6", result.DeviceProfile);
		Assert.Equal("system-images;android-35;google_apis;arm64-v8a", result.SystemImage);
	}

	[Fact]
	public async Task DeleteAvdAsync_CallsDeleteWithCorrectName()
	{
		// Arrange
		string? deletedAvdName = null;
		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.DeleteAvdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Callback<string, CancellationToken>((name, ct) => deletedAvdName = name)
			.Returns(Task.CompletedTask);

		// Act
		await mockProvider.Object.DeleteAvdAsync("MyEmulator");

		// Assert
		Assert.Equal("MyEmulator", deletedAvdName);
		mockProvider.Verify(x => x.DeleteAvdAsync("MyEmulator", It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task StartAvdAsync_StartsWithCorrectOptions()
	{
		// Arrange
		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.StartAvdAsync(
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		await mockProvider.Object.StartAvdAsync("TestEmulator", coldBoot: true, wait: true);

		// Assert
		mockProvider.Verify(x => x.StartAvdAsync("TestEmulator", true, true, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task GetAvdsAsync_ReturnsAvdList()
	{
		// Arrange
		var avds = new List<AvdInfo>
		{
			new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35", DeviceProfile = "pixel_6" },
			new AvdInfo { Name = "Pixel_7_API_34", Target = "android-34", DeviceProfile = "pixel_7" }
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(avds);

		// Act
		var result = await mockProvider.Object.GetAvdsAsync();

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, a => a.Name == "Pixel_6_API_35");
		Assert.Contains(result, a => a.Name == "Pixel_7_API_34");
	}

	[Fact]
	public async Task BootstrapAsync_ReportsProgress()
	{
		// Arrange
		var progressMessages = new List<string>();
		var progress = new Progress<string>(msg => progressMessages.Add(msg));

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.BootstrapAsync(
				It.IsAny<string?>(),
				It.IsAny<string?>(),
				It.IsAny<int>(),
				It.IsAny<IEnumerable<string>?>(),
				It.IsAny<IProgress<string>?>(),
				It.IsAny<CancellationToken>()))
			.Callback<string?, string?, int, IEnumerable<string>?, IProgress<string>?, CancellationToken>(
				(sdk, jdk, ver, pkgs, prog, ct) =>
				{
					prog?.Report("Step 1/4: Installing JDK...");
					prog?.Report("Step 2/4: Installing Android SDK...");
					prog?.Report("Step 3/4: Accepting licenses...");
					prog?.Report("Step 4/4: Installing packages...");
				})
			.Returns(Task.CompletedTask);

		// Act
		await mockProvider.Object.BootstrapAsync(progress: progress);

		// Allow progress callbacks to complete
		await Task.Delay(100);

		// Assert
		Assert.Contains(progressMessages, m => m.Contains("Step 1/4"));
		Assert.Contains(progressMessages, m => m.Contains("Step 2/4"));
		Assert.Contains(progressMessages, m => m.Contains("Step 3/4"));
		Assert.Contains(progressMessages, m => m.Contains("Step 4/4"));
	}

	[Fact]
	public async Task InstallPackagesAsync_InstallsMultiplePackages()
	{
		// Arrange
		IEnumerable<string>? installedPackages = null;
		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.InstallPackagesAsync(
				It.IsAny<IEnumerable<string>>(),
				It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<string>, bool, CancellationToken>((pkgs, accept, ct) => installedPackages = pkgs)
			.Returns(Task.CompletedTask);

		var packages = new[] { "platform-tools", "build-tools;35.0.0", "platforms;android-35" };

		// Act
		await mockProvider.Object.InstallPackagesAsync(packages, acceptLicenses: true);

		// Assert
		Assert.NotNull(installedPackages);
		Assert.Equal(3, installedPackages.Count());
		Assert.Contains("platform-tools", installedPackages);
		Assert.Contains("build-tools;35.0.0", installedPackages);
		Assert.Contains("platforms;android-35", installedPackages);
	}

	[Fact]
	public async Task GetAvailablePackagesAsync_ReturnsAvailablePackages()
	{
		// Arrange
		var availablePackages = new List<SdkPackage>
		{
			new SdkPackage { Path = "platforms;android-36", Version = "1", Description = "Android SDK Platform 36", IsInstalled = false },
			new SdkPackage { Path = "system-images;android-36;google_apis;arm64-v8a", Version = "1", IsInstalled = false },
			new SdkPackage { Path = "build-tools;36.0.0", Version = "36.0.0", IsInstalled = false }
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetAvailablePackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(availablePackages);

		// Act
		var result = await mockProvider.Object.GetAvailablePackagesAsync();

		// Assert
		Assert.Equal(3, result.Count);
		Assert.All(result, pkg => Assert.False(pkg.IsInstalled));
		Assert.Contains(result, p => p.Path == "platforms;android-36");
		Assert.Contains(result, p => p.Path == "system-images;android-36;google_apis;arm64-v8a");
	}

	[Fact]
	public async Task GetInstalledPackagesAsync_SetsIsInstalledToTrue()
	{
		// Arrange
		var installedPackages = new List<SdkPackage>
		{
			new SdkPackage { Path = "platforms;android-35", Version = "3", IsInstalled = true },
			new SdkPackage { Path = "build-tools;35.0.0", Version = "35.0.0", IsInstalled = true },
			new SdkPackage { Path = "platform-tools", Version = "35.0.2", IsInstalled = true }
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetInstalledPackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(installedPackages);

		// Act
		var result = await mockProvider.Object.GetInstalledPackagesAsync();

		// Assert
		Assert.Equal(3, result.Count);
		Assert.All(result, pkg => Assert.True(pkg.IsInstalled));
	}

	[Fact]
	public async Task GetAvailableAndInstalledPackages_CanBeCombined()
	{
		// Arrange
		var installedPackages = new List<SdkPackage>
		{
			new SdkPackage { Path = "platforms;android-35", Version = "3", IsInstalled = true },
			new SdkPackage { Path = "build-tools;35.0.0", Version = "35.0.0", IsInstalled = true }
		};
		var availablePackages = new List<SdkPackage>
		{
			new SdkPackage { Path = "platforms;android-36", Version = "1", IsInstalled = false },
			new SdkPackage { Path = "build-tools;36.0.0", Version = "36.0.0", IsInstalled = false }
		};

		var mockProvider = new Mock<IAndroidProvider>();
		mockProvider.Setup(x => x.GetInstalledPackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(installedPackages);
		mockProvider.Setup(x => x.GetAvailablePackagesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(availablePackages);

		// Act
		var installed = await mockProvider.Object.GetInstalledPackagesAsync();
		var available = await mockProvider.Object.GetAvailablePackagesAsync();
		var allPackages = installed.Concat(available).ToList();

		// Assert
		Assert.Equal(4, allPackages.Count);
		Assert.Equal(2, allPackages.Count(p => p.IsInstalled));
		Assert.Equal(2, allPackages.Count(p => !p.IsInstalled));
	}

	// Helper method to extract API level from system image path
	private static int ExtractApiLevel(string systemImagePath)
	{
		var parts = systemImagePath.Split(';');
		if (parts.Length >= 2)
		{
			var androidPart = parts[1];
			if (androidPart.StartsWith("android-", StringComparison.OrdinalIgnoreCase))
			{
				var levelStr = androidPart.Substring(8);
				if (int.TryParse(levelStr, out var level))
					return level;
			}
		}
		return 0;
	}
}
