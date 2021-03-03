namespace Microsoft.Maui
{
	public interface ILayoutHandler : IViewHandler
	{
		void Add(IView view);
		void Remove(IView view);
	}
}
