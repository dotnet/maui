namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Trait key/values used by xUnitCustomizations.cs to prefix test DisplayNames as
	/// "[Renderer]"/"[Handler]". Shared by Android Shell/Modal/Window and, via the separate
	/// NavigationView* key below, by iOS/MacCatalyst NavigationPage tests.
	/// </summary>
	public static class RendererHandlerVariant
	{
		public const string TraitName = "Variant";
		public const string AndroidShellRenderer = "Renderer";
		public const string AndroidShellHandler = "Handler";

		// Distinct trait key (not reused/collided with TraitName above) for the iOS/MacCatalyst
		// NavigationPage renderer/handler duality, so tests can be searched/filtered by
		// "[NavigationRenderer]"/"[NavigationViewHandler]" independently of the unrelated Android
		// Shell/Modal/Window "Variant" axis. See xUnitCustomizations.cs GetReuseVariantPrefix().
		public const string NavigationViewVariantTraitName = "NavigationViewVariant";
		public const string NavigationRenderer = "NavigationRenderer";
		public const string NavigationViewHandler = "NavigationViewHandler";
	}
}
