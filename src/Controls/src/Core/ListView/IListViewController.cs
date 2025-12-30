#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public interface IListViewController : IViewController
	{
		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		ListViewCachingStrategy CachingStrategy { get; }
		Element FooterElement { get; }
		Element HeaderElement { get; }
		bool RefreshAllowed { get; }

#pragma warning disable CS0618 // Type or member is obsolete
		Cell CreateDefaultCell(object item);
#pragma warning restore CS0618 // Type or member is obsolete
		string GetDisplayTextFromGroup(object cell);
#pragma warning disable CS0618 // Type or member is obsolete
		void NotifyRowTapped(int index, int inGroupIndex, Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void NotifyRowTapped(int index, int inGroupIndex, Cell cell, bool isContextMenuRequested);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void NotifyRowTapped(int index, Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void NotifyRowTapped(int index, Cell cell, bool isContextMenuRequested);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void SendCellAppearing(Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		void SendCellDisappearing(Cell cell);
#pragma warning restore CS0618 // Type or member is obsolete
		void SendRefreshing();
	}
}