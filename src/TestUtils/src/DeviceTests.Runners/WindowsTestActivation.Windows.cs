#nullable enable
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.TestUtils.WindowsTesting;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;
using global::Windows.ApplicationModel;
using global::Windows.ApplicationModel.Activation;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;

internal static class WindowsTestActivation
{
	public static string[] GetArguments(Assembly testAssembly)
	{
		var metadata = testAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
		var packaged = metadata.SingleOrDefault(value => value.Key == "MauiTestPackaged")?.Value;
		var arguments = Environment.GetCommandLineArgs()[1..];
		if (!string.Equals(packaged, "true", StringComparison.OrdinalIgnoreCase))
			return arguments;

		var expected = metadata.SingleOrDefault(value => value.Key == "MauiTestPackageIdentityName")?.Value;
		var publisher = metadata.SingleOrDefault(value => value.Key == "MauiTestPackagePublisher")?.Value;
		if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(publisher) ||
			Package.Current.Id.Name != expected || Package.Current.Id.Publisher != publisher)
			throw new InvalidOperationException("The Windows device tests are not running with their expected MSIX package identity.");

		if (arguments.Contains("--server") && arguments.Any(value => value == "--dotnet-test-pipe" || value.StartsWith("--dotnet-test-pipe=", StringComparison.Ordinal)))
			return arguments;
		if (AppInstance.GetCurrent().GetActivatedEventArgs().Data is ILaunchActivatedEventArgs launch &&
			!string.IsNullOrEmpty(launch.Arguments))
			return WindowsTestCommandLine.ParseActivation(launch.Arguments);
		throw new InvalidOperationException("The packaged test activation did not supply the MTP arguments.");
	}
}
