
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;
using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IVirtualListView : IView
	{
		IVirtualListViewAdapter Adapter { get; }

		IVirtualListViewSelector ViewSelector { get; }

		IView Header { get; }

		IView Footer { get; }

		event EventHandler<SelectedItemsChangedEventArgs> SelectedItemsChanged;
		event EventHandler DataInvalidated;

		SelectionMode SelectionMode { get; }

		IReadOnlyList<ItemPosition> SelectedItems { get; }

		ListOrientation Orientation { get; }

		bool IsItemSelected(int sectionIndex, int itemIndex);

		void SetSelected(params ItemPosition[] paths);

		void SetDeselected(params ItemPosition[] paths);

		void InvalidateData();
	}

	public enum ListOrientation
	{
		Vertical,
		Horizontal
	}
}
