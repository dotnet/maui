using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public interface ITemplatedItemsListScrollToRequestedEventArgs
	{
		object Group { get; }
		object Item { get; }
	}
}
