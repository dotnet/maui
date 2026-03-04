namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	/// <summary>
	/// One-time setup that runs before any tests.
	/// In xUnit, this is done via AssemblyInfo.cs or a static constructor.
	/// The call is made in ModuleInitializer or assembly load.
	/// </summary>
	public static class XamlUnitTestsSetup
	{
		static XamlUnitTestsSetup()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
		}

		public static void Initialize()
		{
			// Trigger the static constructor
		}
	}
}
