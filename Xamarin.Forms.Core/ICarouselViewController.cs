namespace Xamarin.Forms
{
	public interface ICarouselViewController : IItemViewController
	{
		void SendPositionAppearing(int position);
		void SendPositionDisappearing(int position);
		void SendSelectedItemChanged(object item);
		void SendSelectedPositionChanged(int position);
	}
}