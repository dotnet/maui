using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

internal static class ModuleInitializer
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
	}
}
