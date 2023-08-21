// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Items
{
	// Lets observable items sources notify observers about dataset changes
	internal interface ICollectionChangedNotifier
	{
		void NotifyDataSetChanged();
		void NotifyItemChanged(IItemsViewSource source, int startIndex);
		void NotifyItemInserted(IItemsViewSource source, int startIndex);
		void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition);
		void NotifyItemRangeChanged(IItemsViewSource source, int start, int end);
		void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count);
		void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count);
		void NotifyItemRemoved(IItemsViewSource source, int startIndex);
	}
}
