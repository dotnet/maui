namespace System.Maui
{
	public interface IPage : IFrameworkElement
	{
		string Title { get; }

		object Content { get; }
	}
}
