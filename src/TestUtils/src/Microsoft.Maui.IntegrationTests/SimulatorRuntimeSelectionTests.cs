using System.Text.Json;
using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class SimulatorRuntimeSelectionTests
{
	[Fact]
	public void SelectsNewestAvailableRuntimeSupportingXHarnessDevice()
	{
		using var runtimes = JsonDocument.Parse("""
			[
			  { "platform": "iOS", "version": "26.2", "isAvailable": true,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro" }] },
			  { "platform": "iOS", "version": "27.1", "isAvailable": true,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-17" }] },
			  { "platform": "iOS", "version": "28.0", "isAvailable": false,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro" }] },
			  { "platform": "tvOS", "version": "29.0", "isAvailable": true }
			]
			""");

		Assert.Equal(new Version(26, 2), Simulator.SelectLatestIOSVersion(runtimes.RootElement));
	}

	[Fact]
	public void RetainsRuntimeWhenOlderMetadataOmitsDeviceTypes()
	{
		using var runtimes = JsonDocument.Parse("""
			[
			  { "platform": "iOS", "version": "26.4", "isAvailable": true },
			  { "platform": "iOS", "version": "27.1", "isAvailable": true,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-17" }] }
			]
			""");

		Assert.Equal(new Version(26, 4), Simulator.SelectLatestIOSVersion(runtimes.RootElement));
	}

	[Fact]
	public void ReturnsNoRuntimeWhenNoneSupportXHarnessDevice()
	{
		using var runtimes = JsonDocument.Parse("""
			[
			  { "platform": "iOS", "version": "27.1", "isAvailable": true,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-17" }] }
			]
			""");

		Assert.Null(Simulator.SelectLatestIOSVersion(runtimes.RootElement));
	}

	[Fact]
	public void PrefersLatestCompatibleIOS18Runtime()
	{
		using var runtimes = JsonDocument.Parse("""
			[
			  { "platform": "iOS", "version": "18.4", "isAvailable": true },
			  { "platform": "iOS", "version": "18.5", "isAvailable": true,
			    "supportedDeviceTypes": [{ "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro" }] },
			  { "platform": "iOS", "version": "26.2", "isAvailable": true }
			]
			""");

		Assert.Equal(new Version(18, 5), Simulator.SelectLatestIOSVersion(runtimes.RootElement));
	}
}
