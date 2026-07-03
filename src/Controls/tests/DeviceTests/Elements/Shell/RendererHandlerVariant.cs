namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Trait key/values marking Renderer/Handler test classes so xUnitCustomizations.cs can prefix
	/// their DisplayName as "[Renderer]"/"[Handler]". Keep these values in sync with the copies
	/// declared in xUnitCustomizations.cs (that project can't reference this type directly).
	/// </summary>
	public static class RendererHandlerVariant
	{
		public const string TraitName = "Variant";
		public const string Renderer = "Renderer";
		public const string Handler = "Handler";
	}
}
