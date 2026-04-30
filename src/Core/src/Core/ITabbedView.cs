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
		IReadOnlyList<ITab> Tabs { get; }

		/// <summary>
		/// Gets or sets the currently selected tab.
		/// </summary>
		ITab? CurrentTab { get; set; }

		/// <summary>
		/// Gets the background color of the tab bar.
		/// </summary>
		Color? BarBackgroundColor { get; }

		/// <summary>
		/// Gets the background of the tab bar. Typically a Brush or Paint (supports gradients).
		/// </summary>
		object BarBackground { get; }

		/// <summary>
		/// Gets the text color of the tab bar items.
		/// </summary>
		Color? BarTextColor { get; }

		/// <summary>
		/// Gets the color for unselected tab items.
		/// </summary>
		Color? UnselectedTabColor { get; }

		/// <summary>
		/// Gets the color for the selected tab item.
		/// </summary>
		Color? SelectedTabColor { get; }

		/// <summary>
		/// Gets the placement of the tab bar (top or bottom).
		/// </summary>
		TabBarPlacement TabBarPlacement { get; }

		/// <summary>
		/// Gets the number of offscreen pages to retain in the ViewPager.
		/// </summary>
		int OffscreenPageLimit { get; }

		/// <summary>
		/// Gets whether swipe-based paging between tabs is enabled.
		/// </summary>
		bool IsSwipePagingEnabled { get; }

		/// <summary>
		/// Gets whether smooth scroll animation is used when switching tabs.
		/// </summary>
		bool IsSmoothScrollEnabled { get; }

		/// <summary>
		/// Raised when the tab collection changes.
		/// </summary>
		event NotifyCollectionChangedEventHandler? TabsChanged;
	}
}
