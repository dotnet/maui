namespace Microsoft.Maui
{
	public interface ILayoutHandler : IViewHandler
	{
		void Add(IView view);
		void Remove(IView view);
		void Clear();
		void Insert(int index, IView view);
		void Update(int index, IView view);
	}
}
