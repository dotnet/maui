namespace Microsoft.Maui
{
	public interface ILayoutHandler : IFrameworkElementHandler
	{
		void Add(IView view);
		void Remove(IView view);
	}
}
