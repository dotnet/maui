using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace Microsoft.Maui.Controls.Handlers.Items2;

internal partial class MauiItemsViewAutomationPeer(MauiItemsView owner) : ItemsViewAutomationPeer(owner), ISelectionProvider
{
	bool ISelectionProvider.CanSelectMultiple => owner.MauiSelectionMode == SelectionMode.Multiple;
	bool ISelectionProvider.IsSelectionRequired => false;
	IRawElementProviderSimple[] ISelectionProvider.GetSelection() => GetSelection();

	protected override string GetClassNameCore() => nameof(CollectionView);

	protected override object? GetPatternCore(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Scroll &&
			Owner is MauiItemsView itemsView &&
			itemsView.ScrollViewerControl is not null)
		{
			var scrollViewerPeer = FrameworkElementAutomationPeer.CreatePeerForElement(itemsView.ScrollViewerControl);
			return scrollViewerPeer?.GetPattern(patternInterface) ?? base.GetPatternCore(patternInterface);
		}

		return base.GetPatternCore(patternInterface);
	}
}