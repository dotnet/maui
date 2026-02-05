// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Services;
using Moq;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class DeviceManagerTests
{
	[Fact]
	public async Task GetAllDevicesAsync_CombinesAndroidAndAppleDevices()
	{
		// Arrange
		var androidDevices = new List<Device>
		{
			new Device { Id = "emulator-5554", Name = "Pixel 6", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Booted, IsEmulator = true, IsRunning = true }
		};

		var iosDevices = new List<Device>
		{
			new Device { Id = "ABC-123", Name = "iPhone 15", Platforms = new[] { "ios" }, Type = DeviceType.Simulator, State = DeviceState.Shutdown, IsEmulator = true, IsRunning = false }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(androidDevices);
		mockAndroid.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AvdInfo>());

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.ListSimulatorsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(iosDevices);

		var manager = new DeviceManager(mockAndroid.Object, mockApple.Object);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert
		Assert.Equal(2, devices.Count);
		Assert.Contains(devices, d => d.Platforms.Contains("android"));
		Assert.Contains(devices, d => d.Platforms.Contains("ios"));
	}

	[Fact]
	public async Task GetDevicesByPlatformAsync_FiltersCorrectly()
	{
		// Arrange
		var androidDevices = new List<Device>
		{
			new Device { Id = "emulator-5554", Name = "Pixel 6", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Booted, IsEmulator = true, IsRunning = true }
		};

		var iosDevices = new List<Device>
		{
			new Device { Id = "ABC-123", Name = "iPhone 15", Platforms = new[] { "ios" }, Type = DeviceType.Simulator, State = DeviceState.Shutdown, IsEmulator = true, IsRunning = false }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(androidDevices);
		mockAndroid.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AvdInfo>());

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.ListSimulatorsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(iosDevices);

		var manager = new DeviceManager(mockAndroid.Object, mockApple.Object);

		// Act
		var androidOnly = await manager.GetDevicesByPlatformAsync("android");
		var iosOnly = await manager.GetDevicesByPlatformAsync("ios");

		// Assert
		Assert.Single(androidOnly);
		Assert.All(androidOnly, d => Assert.Contains("android", d.Platforms));
		Assert.Single(iosOnly);
		Assert.All(iosOnly, d => Assert.Contains("ios", d.Platforms));
	}

	[Fact]
	public async Task GetDeviceByIdAsync_FindsCorrectDevice()
	{
		// Arrange
		var devices = new List<Device>
		{
			new Device { Id = "device-1", Name = "Device 1", Platforms = new[] { "android" }, Type = DeviceType.Physical, State = DeviceState.Booted, IsEmulator = false, IsRunning = true },
			new Device { Id = "device-2", Name = "Device 2", Platforms = new[] { "android" }, Type = DeviceType.Emulator, State = DeviceState.Shutdown, IsEmulator = true, IsRunning = false }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(devices);
		mockAndroid.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AvdInfo>());

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.ListSimulatorsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Device>());

		var manager = new DeviceManager(mockAndroid.Object, mockApple.Object);

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
		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Device>());
		mockAndroid.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AvdInfo>());

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.ListSimulatorsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Device>());

		var manager = new DeviceManager(mockAndroid.Object, mockApple.Object);

		// Act
		var device = await manager.GetDeviceByIdAsync("nonexistent");

		// Assert
		Assert.Null(device);
	}

	[Fact]
	public async Task GetAllDevicesAsync_IncludesShutdownAvds()
	{
		// Arrange
		var runningDevices = new List<Device>();
		var avds = new List<AvdInfo>
		{
			new AvdInfo { Name = "Pixel_6_API_35", Target = "android-35" }
		};

		var mockAndroid = new Mock<IAndroidProvider>();
		mockAndroid.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(runningDevices);
		mockAndroid.Setup(x => x.GetAvdsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(avds);

		var mockApple = new Mock<IAppleProvider>();
		mockApple.Setup(x => x.ListSimulatorsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Device>());

		var manager = new DeviceManager(mockAndroid.Object, mockApple.Object);

		// Act
		var devices = await manager.GetAllDevicesAsync();

		// Assert
		Assert.Single(devices);
		Assert.Equal("Pixel_6_API_35", devices[0].Id);
		Assert.Equal(DeviceState.Shutdown, devices[0].State);
		Assert.Equal(DeviceType.Emulator, devices[0].Type);
	}
}
