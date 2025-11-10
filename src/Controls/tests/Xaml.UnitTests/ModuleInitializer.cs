using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Hosting;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	internal static class ModuleInitializer
	{
		[ModuleInitializer]
		internal static void Initialize()
		{
			// Enable compatibility for all tests in this assembly
			CompatibilityCheck.UseCompatibility();
		}
	}
}