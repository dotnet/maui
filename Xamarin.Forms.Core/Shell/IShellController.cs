using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public interface IAppearanceObserver
	{
		void OnAppearanceChanged(ShellAppearance appearance);
	}

	public interface IFlyoutBehaviorObserver
	{
		void OnFlyoutBehaviorChanged(FlyoutBehavior behavior);
	}

	public interface IShellController : IPageController
	{
		event EventHandler StructureChanged;

		View FlyoutHeader { get; }

		ImageSource FlyoutIcon { get; }

		void AddAppearanceObserver(IAppearanceObserver observer, Element pivot);

		void AddFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer);

		void AppearanceChanged(Element source, bool appearanceSet);

		List<List<Element>> GenerateFlyoutGrouping();

		ShellNavigationState GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, bool includeStack = true);

		void OnFlyoutItemSelected(Element element);

		Task OnFlyoutItemSelectedAsync(Element element);

		bool ProposeNavigation(ShellNavigationSource source, ShellItem item, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> stack, bool canCancel);

		bool RemoveAppearanceObserver(IAppearanceObserver observer);

		bool RemoveFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer);

		void UpdateCurrentState(ShellNavigationSource source);

		ReadOnlyCollection<ShellItem> GetItems();

		event NotifyCollectionChangedEventHandler ItemsCollectionChanged;
	}
}