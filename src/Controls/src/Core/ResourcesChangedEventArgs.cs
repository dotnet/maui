using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ResourcesChangedEventArgs : EventArgs
	{
		public static readonly ResourcesChangedEventArgs StyleSheets = new ResourcesChangedEventArgs(null);

		public ResourcesChangedEventArgs(IEnumerable<KeyValuePair<string, object>> values)
		{
			Values = values;
		}

		public IEnumerable<KeyValuePair<string, object>> Values { get; private set; }
	}
}