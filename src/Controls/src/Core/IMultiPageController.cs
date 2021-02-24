namespace Microsoft.Maui.Controls
{
	public interface IMultiPageController<T>
	{
		T GetPageByIndex(int index);
	}
}