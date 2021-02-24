using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	public interface ITemplatedItemsView<TItem> : IItemsView<TItem> where TItem : BindableObject
	{
		event PropertyChangedEventHandler PropertyChanged;

		IListProxy ListProxy { get; }
		ITemplatedItemsList<TItem> TemplatedItems { get; }
	}
}
