using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.TestUtils.WindowsTesting;

if (!OperatingSystem.IsWindows())
{
	Console.Error.WriteLine("Installed MSIX activation requires Windows.");
	return 1;
}
if (args.Length < 1)
{
	Console.Error.WriteLine("Expected the owned package state path followed by the unmodified MTP arguments.");
	return 1;
}

try
{
	var path = Path.GetFullPath(args[0]);
	var state = JsonSerializer.Deserialize<PackagedTestState>(File.ReadAllText(path)) ??
		throw new InvalidOperationException("The package session state is empty.");
	using var cancellation = new CancellationTokenSource();
	Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };
	return PackagedTestSession.Run(state, args[1..],
		(aumid, commandLine) =>
		{
			var process = Process.GetProcessById(checked((int)WindowsPackageInterop.Activate(aumid, commandLine)));
			_ = process.Handle;
			return process;
		},
		WindowsPackageInterop.GetProcessPackageFullName,
		WindowsPackageInterop.GetRegisteredPackagePublisher,
		value =>
		{
			var pending = path + "." + Guid.NewGuid().ToString("N") + ".next";
			File.WriteAllText(pending, JsonSerializer.Serialize(value));
			File.Move(pending, path, overwrite: true);
		}, cancellation.Token);
}
catch (Exception exception)
{
	Console.Error.WriteLine(exception);
	return 1;
}
