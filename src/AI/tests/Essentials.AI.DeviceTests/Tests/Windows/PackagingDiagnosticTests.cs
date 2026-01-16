#if WINDOWS
using Microsoft.Extensions.AI;
using Windows.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Trait("Category", "PhiSilica")]
public class PackagingDiagnosticTests
{
	[Fact(DisplayName = "Verify App Is Packaged")]
	public void AppMustBePackaged()
	{
		try
		{
			var packageId = Package.Current.Id;
			var familyName = packageId.FamilyName;
			var packageName = packageId.Name;
			var version = packageId.Version;
			
			Assert.NotNull(familyName);
			Assert.NotNull(packageName);
			
			Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║           Windows Packaging Diagnostic                   ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
			Console.WriteLine($"║  ✅ APP IS PACKAGED                                       ║");
			Console.WriteLine($"║  Package Name:     {packageName,-36} ║");
			Console.WriteLine($"║  Family Name:      {familyName,-36} ║");
			Console.WriteLine($"║  Version:          {version.Major}.{version.Minor}.{version.Build}.{version.Revision,-24} ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
		}
		catch (Exception ex)
		{
			Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║           Windows Packaging Diagnostic                   ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
			Console.WriteLine($"║  ❌ APP IS NOT PACKAGED                                   ║");
			Console.WriteLine($"║  Exception:        {ex.GetType().Name,-36} ║");
			Console.WriteLine($"║  Message:          {ex.Message,-36} ║");
			Console.WriteLine("║                                                           ║");
			Console.WriteLine("║  Windows Copilot Runtime requires packaged MSIX apps!    ║");
			Console.WriteLine("║  See DiagnosePackaging.md for instructions.               ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
			
			Assert.Fail($"App is not packaged. Windows Copilot Runtime requires MSIX packaging. Exception: {ex.Message}");
		}
	}

	[Fact(DisplayName = "Verify Manifest Has SystemAIModels Capability")]
	public async Task ManifestMustHaveSystemAIModelsCapability()
	{
		try
		{
			// Try to access Windows.AI APIs
			var readyState = Microsoft.Windows.AI.Text.LanguageModel.GetReadyState();
			
			Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║           Windows AI Capability Check                    ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
			Console.WriteLine($"║  Ready State: {readyState,-44} ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

			// If we get here without exception, capability is declared
			Assert.True(true, "systemAIModels capability is correctly declared");
		}
		catch (UnauthorizedAccessException ex)
		{
			Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║           Windows AI Capability Check                    ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
			Console.WriteLine($"║  ❌ CAPABILITY NOT DECLARED                               ║");
			Console.WriteLine($"║  Exception:        {ex.GetType().Name,-36} ║");
			Console.WriteLine($"║  Message:          {ex.Message,-36} ║");
			Console.WriteLine("║                                                           ║");
			Console.WriteLine("║  Your Package.appxmanifest must include:                  ║");
			Console.WriteLine("║  <systemai:Capability Name=\"systemAIModels\" />            ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
			
			Assert.Fail($"systemAIModels capability not declared in manifest. Exception: {ex.Message}");
		}
		catch (System.Runtime.InteropServices.COMException ex) when (ex.Message.Contains("Not declared by app", StringComparison.Ordinal))
		{
			Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
			Console.WriteLine("║           Windows AI Capability Check                    ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
			Console.WriteLine($"║  ❌ CAPABILITY NOT DECLARED                               ║");
			Console.WriteLine($"║  HResult:          0x{ex.HResult:X8}                               ║");
			Console.WriteLine($"║  Message:          {ex.Message,-36} ║");
			Console.WriteLine("║                                                           ║");
			Console.WriteLine("║  Your Package.appxmanifest must include:                  ║");
			Console.WriteLine("║  <systemai:Capability Name=\"systemAIModels\" />            ║");
			Console.WriteLine("║                                                           ║");
			Console.WriteLine("║  Also verify you're running as packaged MSIX!             ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
			
			Assert.Fail($"systemAIModels capability not declared or app not packaged. HResult: 0x{ex.HResult:X8}, Message: {ex.Message}");
		}
	}
}
#endif
