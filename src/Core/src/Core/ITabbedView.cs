using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that consists of a list of tabs and a larger detail area, 
	/// with each tab loading content into the detail area.
	/// </summary>
	public interface ITabbedView : IView
	{
		/// <summary>
		/// Gets the collection of tabs.
		/// </summary>
		IReadOnlyList<ITab> Tabs => System.Array.Empty<ITab>();

		/// <summary>
		/// Gets or sets the currently selected tab.
		/// </summary>
		ITab? CurrentTab { get => null; set { } }

		/// <summary>
		/// Gets the background color of the tab bar.
		/// </summary>
		Color? BarBackgroundColor => null;

		/// <summary>
		/// Gets the background of the tab bar. Typically a Brush or Paint (supports gradients).
		/// </summary>
		object BarBackground => null!;

		/// <summary>
		/// Gets the text color of the tab bar items.
		/// </summary>
		Color? BarTextColor => null;

		/// <summary>
		/// Gets the color for unselected tab items.
		/// </summary>
		Color? UnselectedTabColor => null;

		/// <summary>
		/// Gets the color for the selected tab item.
		/// </summary>
		Color? SelectedTabColor => null;

		/// <summary>
		/// Gets the placement of the tab bar (top or bottom).
		/// </summary>
		TabBarPlacement TabBarPlacement => TabBarPlacement.Bottom;

		/// <summary>
		/// Gets the number of offscreen pages to retain in the ViewPager.
		/// </summary>
		int OffscreenPageLimit => 1;

		/// <summary>
		/// Gets whether swipe-based paging between tabs is enabled.
		/// </summary>
		bool IsSwipePagingEnabled => true;

		/// <summary>
		/// Gets whether smooth scroll animation is used when switching tabs.
		/// </summary>
		bool IsSmoothScrollEnabled => true;

		/// <summary>
		/// Raised when the tab collection changes.
		/// </summary>
		event NotifyCollectionChangedEventHandler? TabsChanged { add { } remove { } }
	}
}
