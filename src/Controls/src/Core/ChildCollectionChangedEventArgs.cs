using System;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	internal class ChildCollectionChangedEventArgs : EventArgs
	{
		public ChildCollectionChangedEventArgs(NotifyCollectionChangedEventArgs args)
		{
			Args = args;
		}

		public NotifyCollectionChangedEventArgs Args { get; private set; }
	}
}