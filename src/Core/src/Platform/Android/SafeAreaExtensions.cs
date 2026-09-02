namespace Microsoft.Maui.Platform;

internal static class SafeAreaExtensions
{
	internal static ISafeAreaView2? GetSafeAreaView2(object? layout) =>
		layout switch
		{
			ISafeAreaView2 safeAreaView => safeAreaView,
			IElementHandler { VirtualView: ISafeAreaView2 virtualSafeAreaView } => virtualSafeAreaView,
			_ => null
		};
}
