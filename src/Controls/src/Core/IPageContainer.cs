namespace Microsoft.Maui.Controls
{
	public interface IPageContainer<out T> where T : Page
	{
		T CurrentPage { get; }
	}
}