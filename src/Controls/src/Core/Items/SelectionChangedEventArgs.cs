using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SelectionChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SelectionChangedEventArgs']/Docs" />
	public class SelectionChangedEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectionChangedEventArgs.xml" path="//Member[@MemberName='PreviousSelection']/Docs" />
		public IReadOnlyList<object> PreviousSelection { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/SelectionChangedEventArgs.xml" path="//Member[@MemberName='CurrentSelection']/Docs" />
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