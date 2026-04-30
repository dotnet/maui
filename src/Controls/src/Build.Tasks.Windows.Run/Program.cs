namespace Microsoft.Maui.Windows.Run;

/// <summary>
/// Entry point for the Microsoft.Maui.Windows.Run tool.
///
/// Usage:
///   dotnet exec Microsoft.Maui.Windows.Run.dll register --manifest &lt;path&gt;
///   dotnet exec Microsoft.Maui.Windows.Run.dll run --manifest &lt;path&gt;
///
/// Subcommands:
///   register   Register the MSIX package from a loose file layout (AppxManifest.xml)
///   run        Register (if needed), activate the app, and wait for it to exit
/// </summary>
class Program
{
	static async Task<int> Main(string[] args)
	{
		if (args.Length == 0 || HasFlag(args, "--help", "-h", "-?"))
		{
			PrintUsage();
			return args.Length == 0 ? 1 : 0;
		}

		if (HasFlag(args, "--version"))
		{
			Console.WriteLine(typeof(Program).Assembly.GetName().Version);
			return 0;
		}

		var command = args[0].ToLowerInvariant();
		var manifestPath = GetArgValue(args, "--manifest");

		if (string.IsNullOrEmpty(manifestPath))
		{
			Console.Error.WriteLine("ERROR: --manifest <path> is required.");
			PrintUsage();
			return 1;
		}

		using var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
		};

		try
		{
			return command switch
			{
				"register" => await RunRegisterAsync(manifestPath, cts.Token),
				"run" => await RunLaunchAsync(manifestPath, cts.Token),
				_ => PrintUnknownCommand(command),
			};
		}
		catch (OperationCanceledException)
		{
			return 0;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"ERROR: {ex.Message}");
			return 1;
		}
	}

	static async Task<int> RunRegisterAsync(string manifestPath, CancellationToken ct)
	{
		await PackageRegistrar.RegisterAsync(manifestPath, cancellationToken: ct);
		return 0;
	}

	static async Task<int> RunLaunchAsync(string manifestPath, CancellationToken ct)
	{
		// Read manifest once for both registration and launch
		var manifest = AppxManifestReader.Load(manifestPath);

		// Step 1: Register the package (idempotent)
		var familyName = await PackageRegistrar.RegisterAsync(manifestPath, manifest, ct);

		// Step 2: Compute AUMID and package full name
		var aumid = $"{familyName}!{manifest.ApplicationId}";
		var packageFullName = GetPackageFullName(familyName);

		// Step 3: Activate and wait
		return await AppActivator.ActivateAndWaitAsync(aumid, packageFullName, ct);
	}

	static string? GetPackageFullName(string familyName)
	{
		try
		{
			var pm = new global::Windows.Management.Deployment.PackageManager();
			var packages = pm.FindPackages(familyName);
			return packages.FirstOrDefault()?.Id.FullName;
		}
		catch
		{
			return null;
		}
	}

	static bool HasFlag(string[] args, params string[] flags)
		=> args.Any(a => flags.Contains(a, StringComparer.OrdinalIgnoreCase));

	static string? GetArgValue(string[] args, string name)
	{
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
				return args[i + 1];
		}
		return null;
	}

	static int PrintUnknownCommand(string command)
	{
		Console.Error.WriteLine($"Unknown command: '{command}'");
		PrintUsage();
		return 1;
	}

	static void PrintUsage()
	{
		Console.WriteLine("Microsoft.Maui.Windows.Run - MSIX package registration and launch tool");
		Console.WriteLine();
		Console.WriteLine("Usage:");
		Console.WriteLine("  Microsoft.Maui.Windows.Run <command> --manifest <AppxManifest.xml>");
		Console.WriteLine();
		Console.WriteLine("Commands:");
		Console.WriteLine("  register   Register the MSIX package from loose files");
		Console.WriteLine("  run        Register, launch the app, and wait for exit");
		Console.WriteLine();
		Console.WriteLine("Options:");
		Console.WriteLine("  --manifest <path>  Path to AppxManifest.xml (required)");
		Console.WriteLine("  --help, -h         Show this help message");
		Console.WriteLine("  --version          Show version information");
	}
}
