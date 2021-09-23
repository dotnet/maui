using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public class SelectedItemsChangedEventArgs : EventArgs
	{
		public SelectedItemsChangedEventArgs(
			IReadOnlyList<ItemPosition> previousSelection,
			IReadOnlyList<ItemPosition> newSelection)
			: base()
		{
			PreviousSelection = previousSelection;
			NewSelection = newSelection;
		}

		public IReadOnlyList<ItemPosition> PreviousSelection { get; }

		public IReadOnlyList<ItemPosition> NewSelection { get; }
	}
}