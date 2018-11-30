using System;
using System.Collections.Generic;

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
		event EventHandler HeaderChanged;

		event EventHandler StructureChanged;

		View FlyoutHeader { get; }

		void AddAppearanceObserver(IAppearanceObserver observer, Element pivot);

		void AddFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer);

		void AppearanceChanged(Element source, bool appearanceSet);

		List<List<Element>> GenerateFlyoutGrouping();

		ShellNavigationState GetNavigationState(ShellItem shellItem, ShellSection shellSection, ShellContent shellContent, bool includeStack = true);

		void OnFlyoutItemSelected(Element element);

		bool ProposeNavigation(ShellNavigationSource source, ShellItem item, ShellSection shellSection, ShellContent shellContent, IReadOnlyList<Page> stack, bool canCancel);

		bool RemoveAppearanceObserver(IAppearanceObserver observer);

		bool RemoveFlyoutBehaviorObserver(IFlyoutBehaviorObserver observer);

		void UpdateCurrentState(ShellNavigationSource source);
	}
}