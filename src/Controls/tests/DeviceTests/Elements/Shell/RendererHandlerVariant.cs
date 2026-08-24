namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Trait key/values used by xUnitCustomizations.cs to prefix test DisplayNames as
	/// "[Renderer]"/"[Handler]". Shared by Android Shell/Modal/Window and, via separate keys
	/// below, by iOS/MacCatalyst NavigationPage, FlyoutPage, and TabbedPage variants.
	/// </summary>
	public static class RendererHandlerVariant
	{
		public const string TraitName = "Variant";
		public const string AndroidShellRenderer = "Renderer";
		public const string AndroidShellHandler = "Handler";

		// Distinct trait key for the iOS/MacCatalyst
		// NavigationPage renderer/handler duality, so tests can be searched/filtered by
		// "[NavigationRenderer]"/"[NavigationViewHandler]" independently of the unrelated Android
		// Shell/Modal/Window "Variant" axis. See xUnitCustomizations.cs GetReuseVariantPrefix().
		public const string NavigationViewVariantTraitName = "NavigationViewVariant";
		public const string NavigationRenderer = "NavigationRenderer";
		public const string NavigationViewHandler = "NavigationViewHandler";

		// Distinct trait key for the iOS/MacCatalyst
		// FlyoutPage renderer/handler duality, so tests can be searched/filtered by
		// "[PhoneFlyoutPageRenderer]"/"[FlyoutViewHandler]" independently of the unrelated Android
		// Shell/Modal/Window "Variant" axis. See xUnitCustomizations.cs GetReuseVariantPrefix().
		public const string FlyoutViewVariantTraitName = "FlyoutViewVariant";
		public const string PhoneFlyoutPageRenderer = "PhoneFlyoutPageRenderer";
		public const string FlyoutViewHandler = "FlyoutViewHandler";

		// TabbedPage renderer/handler duality, so tests can be searched/filtered by
		// "[TabbedRenderer]"/"[TabbedViewHandler]" independently of the unrelated Android
		// Shell/Modal/Window "Variant" axis. See xUnitCustomizations.cs GetReuseVariantPrefix().
		public const string TabbedViewVariantTraitName = "TabbedViewVariant";
		public const string TabbedRenderer = "TabbedRenderer";
		public const string TabbedViewHandler = "TabbedViewHandler";
	}
}
