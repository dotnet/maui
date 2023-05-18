#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IItemsViewSource : IDisposable
	{
		int Count { get; }

		int GetPosition(object item);
		object GetItem(int position);

		bool HasHeader { get; set; }
		bool HasFooter { get; set; }

		bool IsHeader(int position);
		bool IsFooter(int position);
	}

	public interface IGroupableItemsViewSource : IItemsViewSource
	{
		(int group, int index) GetGroupAndIndex(int position);
		object GetGroup(int groupIndex);
		IItemsViewSource GetGroupItemsViewSource(int groupIndex);

		bool IsGroupHeader(int position);
		bool IsGroupFooter(int position);
	}
}