namespace Microsoft.Maui.Handlers
{
	public interface IMenuBarItemHandler : IElementHandler
	{
		void Add(IMenuFlyoutItemBase view);
		void Remove(IMenuFlyoutItemBase view);
		void Clear();
		void Insert(int index, IMenuFlyoutItemBase view);
	}
}
