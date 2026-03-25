// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Services;
using Microsoft.Maui.Client.UnitTests.Fakes;
using Xunit;

namespace Microsoft.Maui.Client.UnitTests;

public class DeviceManagerTests
{
	[Fact]
	public async Task GetAllDevicesAsync_ReturnsAndroidDevices()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider
		{
			Devices = new List<Device>
			{
				new Device { Id = "emulator-5554", Name = "Pixel 6", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Booted, IsEmulator = true, IsRunning = true }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert
		Assert.Single(devices);
		Assert.Contains(devices, d => d.Platforms.Contains("android"));
	}

	[Fact]
	public async Task GetDevicesByPlatformAsync_FiltersCorrectly()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider
		{
			Devices = new List<Device>
			{
				new Device { Id = "emulator-5554", Name = "Pixel 6", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Booted, IsEmulator = true, IsRunning = true }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var androidOnly = await manager.GetDevicesByPlatformAsync("android");

		// Assert
		Assert.Single(androidOnly);
		Assert.All(androidOnly, d => Assert.Contains("android", d.Platforms));
	}

	[Fact]
	public async Task GetDeviceByIdAsync_FindsCorrectDevice()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider
		{
			Devices = new List<Device>
			{
				new Device { Id = "device-1", Name = "Device 1", Platforms = new[] { "android" }, Type = DeviceType.Physical, State = DeviceState.Booted, IsEmulator = false, IsRunning = true },
				new Device { Id = "device-2", Name = "Device 2", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Shutdown, IsEmulator = true, IsRunning = false }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var device = await manager.GetDeviceByIdAsync("device-2");

		// Assert
		Assert.NotNull(device);
		Assert.Equal("device-2", device.Id);
		Assert.Equal("Device 2", device.Name);
	}

	[Fact]
	public async Task GetDeviceByIdAsync_ReturnsNull_WhenNotFound()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider();
		var manager = new DeviceManager(fakeAndroid);

		// Act
		var device = await manager.GetDeviceByIdAsync("nonexistent");

		// Assert
		Assert.Null(device);
	}

	[Fact]
	public async Task GetAllDevicesAsync_IncludesShutdownAvds()
	{
		// Arrange
		var fakeAndroid = new FakeAndroidProvider
		{
			Avds = new List<AvdInfo>
			{
				new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35" }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert
		Assert.Single(devices);
		Assert.Equal("Pixel_6_API_35", devices[0].Id);
		Assert.Equal(DeviceState.Shutdown, devices[0].State);
		Assert.Equal(DeviceType.Emulator, devices[0].Type);
	}

	[Fact]
	public async Task GetAllDevicesAsync_MergesRunningEmulatorWithAvd()
	{
		// Arrange: ADB returns a running emulator with AVD name in details
		var fakeAndroid = new FakeAndroidProvider
		{
			Devices = new List<Device>
			{
				new Device
				{
					Id = "emulator-5554",
					Name = "Google sdk_gphone64_arm64",
					Platforms = new[] { "android" },
					Type = DeviceType.Emulator,
					State = DeviceState.Booted,
					IsEmulator = true,
					IsRunning = true,
					EmulatorId = "Pixel_6_API_35",
					Details = new Dictionary<string, object> { ["avd"] = "Pixel_6_API_35" }
				}
			},
			Avds = new List<AvdInfo>
			{
				new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35", DeviceProfile = "pixel_6" }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert: should be merged into a single entry, not two
		Assert.Single(devices);
		Assert.Equal("emulator-5554", devices[0].Id);
		Assert.Equal("Pixel_6_API_35", devices[0].EmulatorId);
		Assert.True(devices[0].IsRunning);
	}

	[Fact]
	public async Task GetAllDevicesAsync_MergesRunningEmulatorWithAvd_ByEmulatorId()
	{
		// Arrange: ADB returns a running emulator with EmulatorId set but no "avd" in Details
		var fakeAndroid = new FakeAndroidProvider
		{
			Devices = new List<Device>
			{
				new Device
				{
					Id = "emulator-5554",
					Name = "Google sdk_gphone64_arm64",
					Platforms = new[] { "android" },
					Type = DeviceType.Emulator,
					State = DeviceState.Booted,
					IsEmulator = true,
					IsRunning = true,
					EmulatorId = "Pixel_6_API_35",
					Details = new Dictionary<string, object>()
				}
			},
			Avds = new List<AvdInfo>
			{
				new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35", DeviceProfile = "pixel_6" }
			}
		};

		var manager = new DeviceManager(fakeAndroid);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert: should still merge via EmulatorId fallback
		Assert.Single(devices);
		Assert.Equal("emulator-5554", devices[0].Id);
		Assert.Equal("Pixel_6_API_35", devices[0].EmulatorId);
		Assert.True(devices[0].IsRunning);
	}
}
