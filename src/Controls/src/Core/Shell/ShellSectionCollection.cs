using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Microsoft.Maui.Controls
{

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[DebuggerTypeProxy(typeof(ShellSectionCollectionDebugView))]
	internal sealed class ShellSectionCollection : ShellElementCollection<ShellSection>
	{
		public ShellSectionCollection() : base() { }

		string GetDebuggerDisplay() => $"Count = {Count}";

		/// <summary>
		/// Provides a debug view for the <see cref="ShellSectionCollection"/> class.
		/// </summary>
		/// <param name="collection">The <see cref="ShellSectionCollection"/> instance to debug.</param>
		private sealed class ShellSectionCollectionDebugView(ShellSectionCollection collection)
		{
			public IList VisibleItems => collection.VisibleItems;

			public IReadOnlyCollection<ShellSection> VisibleItemsReadOnly => collection.VisibleItemsReadOnly;
		}
	}
}