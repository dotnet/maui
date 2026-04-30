using global::Windows.Management.Deployment;

namespace Microsoft.Maui.Windows.Run;

/// <summary>
/// Registers an MSIX package from a loose file layout using the Windows PackageManager API.
/// Requires Developer Mode to be enabled on the machine.
/// </summary>
internal static class PackageRegistrar
{
	/// <summary>
	/// Registers the package defined by the AppxManifest.xml at the given path.
	/// Uses DevelopmentMode to allow loose-file registration without signing.
	/// </summary>
	/// <returns>The PackageFamilyName of the registered package.</returns>
	public static async Task<string> RegisterAsync(string manifestPath, AppxManifestReader? manifest = null, CancellationToken cancellationToken = default)
	{
		manifest ??= AppxManifestReader.Load(manifestPath);
		var manifestUri = new Uri(Path.GetFullPath(manifestPath));

		Console.WriteLine($"Registering MSIX package: {manifest.IdentityName} v{manifest.Version} ({manifest.ProcessorArchitecture})");

		var packageManager = new PackageManager();

		try
		{
			// RegisterPackageAsync is for loose-file layouts (AppxManifest.xml path).
			// AddPackageByUriAsync is for packaged .appx/.msix files — do NOT use it here.
			var result = await packageManager.RegisterPackageAsync(
				manifestUri,
				null,
				DeploymentOptions.DevelopmentMode | DeploymentOptions.ForceApplicationShutdown
			).AsTask(cancellationToken);

			if (result.IsRegistered)
			{
				Console.WriteLine($"Package registered successfully.");
			}
			else
			{
				var errorText = result.ErrorText ?? "Unknown error";
				throw new InvalidOperationException(
					$"Failed to register package: {errorText} (0x{result.ExtendedErrorCode?.HResult:X8})");
			}
		}
		catch (Exception ex) when (IsDeveloperModeError(ex))
		{
			Console.Error.WriteLine("ERROR: Windows Developer Mode is required to register MSIX packages from loose files.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("To enable Developer Mode:");
			Console.Error.WriteLine("  1. Open Settings > System > For Developers (or Privacy & Security > For Developers)");
			Console.Error.WriteLine("  2. Enable 'Developer Mode'");
			Console.Error.WriteLine();
			Console.Error.WriteLine($"Underlying error: {ex.Message}");
			Environment.Exit(1);
			return string.Empty; // unreachable
		}

		// Query PackageManager to get the PackageFamilyName
		var packages = packageManager.FindPackagesForUser(string.Empty, manifest.IdentityName, manifest.Publisher);
		var pkg = packages.FirstOrDefault()
			?? throw new InvalidOperationException(
				$"Package '{manifest.IdentityName}' was registered but could not be found. " +
				"This may indicate a publisher mismatch.");

		var familyName = pkg.Id.FamilyName;
		Console.WriteLine($"Package family: {familyName}");

		return familyName;
	}

	static bool IsDeveloperModeError(Exception ex)
	{
		// HRESULT 0x80073CFB = ERROR_PACKAGE_NOT_REGISTERED_FOR_SIDELOAD
		// HRESULT 0x800704EC = ERROR_ACCESS_DISABLED_BY_POLICY (Group Policy blocks sideloading)
		const int ERROR_PACKAGE_NOT_REGISTERED_FOR_SIDELOAD = unchecked((int)0x80073CFB);
		const int ERROR_ACCESS_DISABLED_BY_POLICY = unchecked((int)0x800704EC);

		return ex.HResult == ERROR_PACKAGE_NOT_REGISTERED_FOR_SIDELOAD
			|| ex.HResult == ERROR_ACCESS_DISABLED_BY_POLICY
			|| ex.Message.Contains("sideload", StringComparison.OrdinalIgnoreCase)
			|| ex.Message.Contains("developer mode", StringComparison.OrdinalIgnoreCase);
	}
}
