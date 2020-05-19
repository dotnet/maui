namespace System.Maui
{
	public interface IMultiPageController<T>
	{
		T GetPageByIndex(int index);
	}
}