using System;

namespace Xamarin.Forms
{
	public interface IListViewController : IViewController
	{
		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		ListViewCachingStrategy CachingStrategy { get; }
		Element FooterElement { get; }
		Element HeaderElement { get; }
		bool RefreshAllowed { get; }

		Cell CreateDefaultCell(object item);
		string GetDisplayTextFromGroup(object cell);
		void NotifyRowTapped(int index, int inGroupIndex, Cell cell);
		void NotifyRowTapped(int index, Cell cell);
		void SendCellAppearing(Cell cell);
		void SendCellDisappearing(Cell cell);
		void SendRefreshing();
	}
}