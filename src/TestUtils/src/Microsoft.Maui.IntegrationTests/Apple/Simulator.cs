using System.Text.Json;
using Xunit;

namespace Microsoft.Maui.IntegrationTests.Apple;

public class Simulator : IDisposable
{
	readonly ITestOutputHelper? _output;
	string? _udid;
	bool _created;

	public Simulator(ITestOutputHelper? output = null)
	{
		_output = output;
		if (!string.IsNullOrEmpty(TestEnvironment.IosTestDeviceUdid))
		{
			if (!Guid.TryParse(TestEnvironment.IosTestDeviceUdid, out _))
				throw new ArgumentException("IOS_TEST_DEVICE_UDID must be a dedicated simulator UUID.");
			_udid = TestEnvironment.IosTestDeviceUdid;
		}
	}

	public string GetUDID()
	{
		if (_udid is not null)
			return _udid;

		var json = ToolRunner.Run("xcrun", "simctl list runtimes --json", out var exitCode, timeoutInSeconds: 30, output: _output);
		if (exitCode != 0)
			throw new InvalidOperationException($"Unable to list simulator runtimes: {json}");

		var (runtime, deviceType) = SelectRuntime(json, TestEnvironment.IosTestDevice);
		var name = $"MAUI Integration {Guid.NewGuid():N}";
		var result = ToolRunner.Run("xcrun", $"simctl create \"{name}\" {deviceType} {runtime}", out exitCode,
			timeoutInSeconds: 60, output: _output).Trim();
		if (exitCode != 0 || !Guid.TryParse(result, out _))
			throw new InvalidOperationException($"Unable to create integration simulator: {result}");

		_udid = result;
		_created = true;
		return _udid;
	}

	internal static (string Runtime, string DeviceType) SelectRuntime(string json, string? target)
	{
		string? requestedVersion = null;
		if (!string.IsNullOrEmpty(target))
		{
			var parts = target.Split('_', 2);
			if (parts[0] != "ios-simulator-64" || (parts.Length == 2 && !Version.TryParse(parts[1], out _)))
				throw new ArgumentException($"Unsupported IOS_TEST_DEVICE '{target}'. Expected ios-simulator-64[_major.minor].");
			requestedVersion = parts.Length == 2 ? parts[1] : null;
		}

		using var doc = JsonDocument.Parse(json);
		var candidates = doc.RootElement.GetProperty("runtimes").EnumerateArray()
			.Where(r => r.GetProperty("isAvailable").GetBoolean() &&
				r.GetProperty("identifier").GetString()!.Contains(".iOS-", StringComparison.Ordinal))
			.Where(r => requestedVersion is null ||
				r.GetProperty("version").GetString() == requestedVersion ||
				r.GetProperty("version").GetString()!.StartsWith(requestedVersion + ".", StringComparison.Ordinal))
			.OrderByDescending(r => Version.Parse(r.GetProperty("version").GetString()!));
		foreach (var runtime in candidates)
		{
			var device = runtime.GetProperty("supportedDeviceTypes").EnumerateArray()
				.FirstOrDefault(d => d.GetProperty("productFamily").GetString() == "iPhone");
			if (device.ValueKind != JsonValueKind.Undefined)
				return (runtime.GetProperty("identifier").GetString()!, device.GetProperty("identifier").GetString()!);
		}
		throw new InvalidOperationException($"No available iPhone simulator runtime matches '{target ?? "latest"}'. Install one with Xcode.");
	}

	public bool Launch()
	{
		var text = ToolRunner.Run("xcrun", $"simctl boot {GetUDID()}", out var exitCode, timeoutInSeconds: 30, output: _output);
		return exitCode == 0 || (exitCode == 149 && text.Contains("current state: Booted", StringComparison.Ordinal));
	}

	public bool Shutdown()
	{
		if (_udid is null)
			return true;
		ToolRunner.Run("xcrun", $"simctl shutdown {_udid}", out var exitCode, timeoutInSeconds: 60, output: _output);
		return exitCode == 0;
	}

	public bool WaitForBootComplete(int timeoutSeconds = 120)
	{
		ToolRunner.Run("xcrun", $"simctl bootstatus {GetUDID()}", out var exitCode, timeoutInSeconds: timeoutSeconds, output: _output);
		return exitCode == 0;
	}

	public bool ShowWindow()
	{
		ToolRunner.Run("open", "-a Simulator", out var exitCode, timeoutInSeconds: 30, output: _output);
		return exitCode == 0;
	}

	public void Dispose()
	{
		Shutdown();
		if (_created)
		{
			ToolRunner.Run("xcrun", $"simctl delete {_udid}", out _, timeoutInSeconds: 60, output: _output);
			_udid = null;
			_created = false;
		}
	}
}
