﻿namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IMauiRecyclerView<TItemsView>  where TItemsView : ItemsView
	{
		public void SetUpNewElement(TItemsView newElement);

		public void TearDownOldElement(TItemsView oldElement);

		public void UpdateItemTemplate();

		public void UpdateSource();

		public void UpdateScrollingMode();

		public void UpdateVerticalScrollBarVisibility();

		public void UpdateHorizontalScrollBarVisibility();

		public void UpdateFlowDirection();

		public void UpdateEmptyView();

		public void UpdateLayoutManager();

		public void UpdateAdapter();

	}
}
