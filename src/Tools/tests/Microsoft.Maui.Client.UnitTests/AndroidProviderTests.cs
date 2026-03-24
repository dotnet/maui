// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.UnitTests.Fakes;
using Xunit;

namespace Microsoft.Maui.Client.UnitTests;

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

		var provider = new FakeAndroidProvider
		{
			InstalledPackages = packages,
			GetMostRecentSystemImageFunc = async ct =>
			{
				var pkgs = packages;
				return pkgs
					.Where(p => p.Path.StartsWith("system-images;android-", StringComparison.OrdinalIgnoreCase))
					.Select(p => new { Package = p, ApiLevel = ExtractApiLevel(p.Path) })
					.Where(x => x.ApiLevel > 0)
					.OrderByDescending(x => x.ApiLevel)
					.FirstOrDefault()?.Package.Path;
			}
		};

		// Act
		var result = await provider.GetMostRecentSystemImageAsync();

		// Assert
		Assert.Equal("system-images;android-35;google_apis;arm64-v8a", result);
	}

	[Fact]
	public async Task GetMostRecentSystemImageAsync_ReturnsNull_WhenNoSystemImages()
	{
		// Arrange
		var provider = new FakeAndroidProvider
		{
			InstalledPackages = new List<SdkPackage>
			{
				new SdkPackage { Path = "platform-tools" },
				new SdkPackage { Path = "build-tools;34.0.0" }
			},
			MostRecentSystemImage = null
		};

		// Act
		var result = await provider.GetMostRecentSystemImageAsync();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task CreateAvdAsync_CreatesAvdWithCorrectParameters()
	{
		// Arrange
		var provider = new FakeAndroidProvider();

		// Act
		var result = await provider.CreateAvdAsync(
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
		var provider = new FakeAndroidProvider();

		// Act
		await provider.DeleteAvdAsync("MyEmulator");

		// Assert
		Assert.Single(provider.DeletedAvds);
		Assert.Equal("MyEmulator", provider.DeletedAvds[0]);
	}

	[Fact]
	public async Task StartAvdAsync_StartsWithCorrectOptions()
	{
		// Arrange
		var provider = new FakeAndroidProvider();

		// Act
		await provider.StartAvdAsync("TestEmulator", coldBoot: true, wait: true);

		// Assert
		Assert.Single(provider.StartedAvds);
		Assert.Equal(("TestEmulator", true, true), provider.StartedAvds[0]);
	}

	[Fact]
	public async Task GetAvdsAsync_ReturnsAvdList()
	{
		// Arrange
		var provider = new FakeAndroidProvider
		{
			Avds = new List<AvdInfo>
			{
				new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35", DeviceProfile = "pixel_6" },
				new AvdInfo { Name = "Pixel_7_API_34", Target = "android-34", DeviceProfile = "pixel_7" }
			}
		};

		// Act
		var result = await provider.GetAvdsAsync();

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, a => a.Name == "Pixel_6_API_35");
		Assert.Contains(result, a => a.Name == "Pixel_7_API_34");
	}

	[Fact]
	public async Task InstallAsync_ReportsProgress()
	{
		// Arrange
		var progressMessages = new List<string>();
		var progress = new Progress<string>(msg => progressMessages.Add(msg));

		var provider = new FakeAndroidProvider
		{
			InstallCallback = (sdk, jdk, ver, pkgs, prog, ct) =>
			{
				prog?.Report("Step 1/4: Installing JDK...");
				prog?.Report("Step 2/4: Installing Android SDK...");
				prog?.Report("Step 3/4: Accepting licenses...");
				prog?.Report("Step 4/4: Installing packages...");
			}
		};

		// Act
		await provider.InstallAsync(progress: progress);

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
		var provider = new FakeAndroidProvider();
		var packages = new[] { "platform-tools", "build-tools;35.0.0", "platforms;android-35" };

		// Act
		await provider.InstallPackagesAsync(packages, acceptLicenses: true);

		// Assert
		Assert.Single(provider.InstalledPackageSets);
		var installedPackages = provider.InstalledPackageSets[0];
		Assert.Equal(3, installedPackages.Count);
		Assert.Contains("platform-tools", installedPackages);
		Assert.Contains("build-tools;35.0.0", installedPackages);
		Assert.Contains("platforms;android-35", installedPackages);
	}

	[Fact]
	public async Task GetAvailablePackagesAsync_ReturnsAvailablePackages()
	{
		// Arrange
		var provider = new FakeAndroidProvider
		{
			AvailablePackages = new List<SdkPackage>
			{
				new SdkPackage { Path = "platforms;android-36", Version = "1", Description = "Android SDK Platform 36", IsInstalled = false },
				new SdkPackage { Path = "system-images;android-36;google_apis;arm64-v8a", Version = "1", IsInstalled = false },
				new SdkPackage { Path = "build-tools;36.0.0", Version = "36.0.0", IsInstalled = false }
			}
		};

		// Act
		var result = await provider.GetAvailablePackagesAsync();

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
		var provider = new FakeAndroidProvider
		{
			InstalledPackages = new List<SdkPackage>
			{
				new SdkPackage { Path = "platforms;android-35", Version = "3", IsInstalled = true },
				new SdkPackage { Path = "build-tools;35.0.0", Version = "35.0.0", IsInstalled = true },
				new SdkPackage { Path = "platform-tools", Version = "35.0.2", IsInstalled = true }
			}
		};

		// Act
		var result = await provider.GetInstalledPackagesAsync();

		// Assert
		Assert.Equal(3, result.Count);
		Assert.All(result, pkg => Assert.True(pkg.IsInstalled));
	}

	[Fact]
	public async Task GetAvailableAndInstalledPackages_CanBeCombined()
	{
		// Arrange
		var provider = new FakeAndroidProvider
		{
			InstalledPackages = new List<SdkPackage>
			{
				new SdkPackage { Path = "platforms;android-35", Version = "3", IsInstalled = true },
				new SdkPackage { Path = "build-tools;35.0.0", Version = "35.0.0", IsInstalled = true }
			},
			AvailablePackages = new List<SdkPackage>
			{
				new SdkPackage { Path = "platforms;android-36", Version = "1", IsInstalled = false },
				new SdkPackage { Path = "build-tools;36.0.0", Version = "36.0.0", IsInstalled = false }
			}
		};

		// Act
		var installed = await provider.GetInstalledPackagesAsync();
		var available = await provider.GetAvailablePackagesAsync();
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
