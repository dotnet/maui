namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Trait key/values for Android Shell Renderer/Handler test variants so that, on Android only,
	/// xUnitCustomizations.cs can prefix their DisplayName as "[Renderer]"/"[Handler]".
	/// Also reused by Modal/Window tests, which share the same Android Renderer/Handler reuse logic as Shell.
	/// </summary>
	public static class RendererHandlerVariant
	{
		public const string TraitName = "Variant";
		public const string AndroidShellRenderer = "Renderer";
		public const string AndroidShellHandler = "Handler";
	}
}
