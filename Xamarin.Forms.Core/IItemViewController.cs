namespace Xamarin.Forms
{
	public interface IItemViewController
	{
		void BindView(View view, object item);
		View CreateView(object itemType);
		object GetItem(int index);
		object GetItemType(object item);
		int Count
		{
			get;
		}
	}
}