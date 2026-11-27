using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class SimulatorTests
{
	const string Runtimes = """
		{"runtimes":[
		  {"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-18-6","version":"18.6","isAvailable":true,
		   "supportedDeviceTypes":[{"productFamily":"iPhone","identifier":"phone18"}]},
		  {"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-27-0","version":"27.0","isAvailable":true,
		   "supportedDeviceTypes":[{"productFamily":"iPad","identifier":"pad27"},{"productFamily":"iPhone","identifier":"phone27"}]},
		  {"identifier":"com.apple.CoreSimulator.SimRuntime.iOS-28-0","version":"28.0","isAvailable":false},
		  {"identifier":"com.apple.CoreSimulator.SimRuntime.tvOS-29-0","version":"29.0","isAvailable":true}
		]}
		""";

	[Theory]
	[InlineData(null, "com.apple.CoreSimulator.SimRuntime.iOS-27-0", "phone27")]
	[InlineData("ios-simulator-64", "com.apple.CoreSimulator.SimRuntime.iOS-27-0", "phone27")]
	[InlineData("ios-simulator-64_18.6", "com.apple.CoreSimulator.SimRuntime.iOS-18-6", "phone18")]
	public void SelectsAvailableRuntimeAndCompatibleIPhone(string? target, string runtime, string device) =>
		Assert.Equal((runtime, device), Simulator.SelectRuntime(Runtimes, target));

	[Fact]
	public void RejectsMissingRequestedRuntime() =>
		Assert.Throws<InvalidOperationException>(() => Simulator.SelectRuntime(Runtimes, "ios-simulator-64_17.0"));

	[Fact]
	public void RejectsInvalidTarget() =>
		Assert.Throws<ArgumentException>(() => Simulator.SelectRuntime(Runtimes, "not-a-simulator"));
}
