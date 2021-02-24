using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public class SelectionChangedEventArgs : EventArgs
	{
		public IReadOnlyList<object> PreviousSelection { get; }
		public IReadOnlyList<object> CurrentSelection { get; }

		static readonly IReadOnlyList<object> s_empty = new List<object>(0);

		internal SelectionChangedEventArgs(object previousSelection, object currentSelection)
		{
			PreviousSelection = previousSelection != null ? new List<object>(1) { previousSelection } : s_empty;
			CurrentSelection = currentSelection != null ? new List<object>(1) { currentSelection } : s_empty;
		}

		internal SelectionChangedEventArgs(IList<object> previousSelection, IList<object> currentSelection)
		{
			PreviousSelection = new List<object>(previousSelection ?? throw new ArgumentNullException(nameof(previousSelection)));
			CurrentSelection = new List<object>(currentSelection ?? throw new ArgumentNullException(nameof(currentSelection)));
		}
	}
}