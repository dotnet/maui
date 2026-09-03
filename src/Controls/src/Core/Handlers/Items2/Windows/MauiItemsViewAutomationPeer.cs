using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace Microsoft.Maui.Controls.Handlers.Items2;

// Explicitly implements ISelectionProvider (rather than relying on the base
// ItemsViewAutomationPeer's implementation alone) so C#/WinRT projects a valid
// COM identity for PatternInterface.Selection. Without this, WinUI's Down-arrow
// single-selection path can throw "No such interface supported" (E_NOINTERFACE)
// when it queries the peer for ISelectionProvider.
internal partial class MauiItemsViewAutomationPeer(MauiItemsView owner) : ItemsViewAutomationPeer(owner), ISelectionProvider
{
	protected override string GetClassNameCore() => nameof(CollectionView);

	protected override object? GetPatternCore(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Selection &&
			Owner is MauiItemsView { SelectionMode: not UI.Xaml.Controls.ItemsViewSelectionMode.None })
		{
			return this;
		}

		if (patternInterface == PatternInterface.Scroll &&
			Owner is MauiItemsView itemsView &&
			itemsView.ScrollViewerControl is not null)
		{
			var scrollViewerPeer = FrameworkElementAutomationPeer.CreatePeerForElement(itemsView.ScrollViewerControl);
			return scrollViewerPeer?.GetPattern(patternInterface) ?? base.GetPatternCore(patternInterface);
		}

		return base.GetPatternCore(patternInterface);
	}

	bool ISelectionProvider.CanSelectMultiple => base.CanSelectMultiple;

	bool ISelectionProvider.IsSelectionRequired => base.IsSelectionRequired;

	IRawElementProviderSimple[] ISelectionProvider.GetSelection() => base.GetSelection();
}