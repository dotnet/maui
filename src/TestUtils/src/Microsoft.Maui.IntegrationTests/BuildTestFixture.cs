using System.Globalization;

namespace Microsoft.Maui.IntegrationTests
{
	/// <summary>
	/// Shared fixture for integration tests that handles one-time setup/teardown.
	/// Implements xUnit's IAsyncLifetime for collection fixture pattern.
	/// </summary>
	public class BuildTestFixture : IDisposable
	{
		public string TestNuGetConfig { get; }

		public BuildTestFixture()
		{
			TestNuGetConfig = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "NuGet.config");
			SetupNuGetConfig();
		}

		/// <summary>
		/// Copy NuGet packages that are not installed as part of the workload and set up NuGet.config
		/// See: `PrepareSeparateBuildContext` in `eng/cake/dotnet.cake`.
		/// </summary>
		private void SetupNuGetConfig()
		{
			string[] NuGetOnlyPackages = new string[] {
				"Microsoft.Maui.Controls.*.nupkg",
				"Microsoft.Maui.Core.*.nupkg",
				"Microsoft.Maui.Essentials.*.nupkg",
				"Microsoft.Maui.Graphics.*.nupkg",
				"Microsoft.Maui.Maps.*.nupkg",
				"Microsoft.Maui.Resizetizer.*.nupkg",
				"Microsoft.AspNetCore.Components.WebView.*.nupkg",
			};

			var mauiDir = TestEnvironment.GetMauiDirectory();
			var artifactDir = Path.Combine(mauiDir, "artifacts");
			if (!Directory.Exists(artifactDir))
				throw new DirectoryNotFoundException($"Build artifact directory '{artifactDir}' was not found.");

			var extraPacksDir = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "extra-packages");
			if (Directory.Exists(extraPacksDir))
				Directory.Delete(extraPacksDir, true);

			Directory.CreateDirectory(extraPacksDir);

			foreach (var searchPattern in NuGetOnlyPackages)
			{
				// First, try artifacts/ root (CI layout where packages are downloaded directly)
				var packages = Directory.GetFiles(artifactDir, searchPattern).ToList();

				// If not found and running locally, try artifacts/packages/*/Shipping/ (local dotnet cake build layout)
				if (packages.Count == 0 && !TestEnvironment.IsRunningOnCI)
				{
					var packagesDir = Path.Combine(artifactDir, "packages");
					if (Directory.Exists(packagesDir))
					{
						packages = Directory.GetFiles(packagesDir, searchPattern, SearchOption.AllDirectories).ToList();
					}
				}

				// If still not found locally, try .dotnet/library-packs/ (installed workload packages)
				if (packages.Count == 0 && !TestEnvironment.IsRunningOnCI)
				{
					var libraryPacksDir = Path.Combine(mauiDir, ".dotnet", "library-packs");
					if (Directory.Exists(libraryPacksDir))
					{
						packages = Directory.GetFiles(libraryPacksDir, searchPattern).ToList();
					}
				}

				foreach (var pack in packages)
					File.Copy(pack, Path.Combine(extraPacksDir, Path.GetFileName(pack)));
			}

			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "NuGet.config"), TestNuGetConfig, true);
			FileUtilities.ReplaceInFile(TestNuGetConfig, "<add key=\"nuget-only\" value=\"true\" />", "");
			FileUtilities.ReplaceInFile(TestNuGetConfig, "NUGET_ONLY_PLACEHOLDER", extraPacksDir);
		}

		public void Dispose()
		{
			// One-time teardown logic if needed
		}
	}

	/// <summary>
	/// Collection definition for integration tests that share the BuildTestFixture.
	/// </summary>
	[CollectionDefinition("IntegrationTests")]
	public class IntegrationTestCollection : ICollectionFixture<BuildTestFixture>
	{
		// This class has no code, and is never created.
		// Its purpose is to be the place to apply [CollectionDefinition] and ICollectionFixture<>.
	}
}
