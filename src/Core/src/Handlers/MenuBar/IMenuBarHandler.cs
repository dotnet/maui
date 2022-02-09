namespace Microsoft.Maui.Handlers
{
	public interface IMenuBarHandler : IElementHandler
	{
		void Add(IMenuBarItem view);
		void Remove(IMenuBarItem view);
		void Clear();
		void Insert(int index, IMenuBarItem view);
	}
}
