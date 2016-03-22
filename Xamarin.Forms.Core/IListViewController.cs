namespace Xamarin.Forms
{
	internal interface IListViewController : IViewController
	{
		Element FooterElement { get; }

		Element HeaderElement { get; }

		bool RefreshAllowed { get; }

		void SendCellAppearing(Cell cell);
		void SendCellDisappearing(Cell cell);
		void SendRefreshing();
	}
}