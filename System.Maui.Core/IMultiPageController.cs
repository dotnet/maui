namespace Xamarin.Forms
{
	public interface IMultiPageController<T>
	{
		T GetPageByIndex(int index);
	}
}