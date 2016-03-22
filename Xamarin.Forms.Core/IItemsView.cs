namespace Xamarin.Forms
{
	internal interface IItemsView<T> where T : BindableObject
	{
		T CreateDefault(object item);
		void SetupContent(T content, int index);
		void UnhookContent(T content);
	}
}