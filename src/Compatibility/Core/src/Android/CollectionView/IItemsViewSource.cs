using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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
		bool IsGroupHeader(int position);
		bool IsGroupFooter(int position);
	}
}