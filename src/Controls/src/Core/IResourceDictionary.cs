using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IResourceDictionary : IEnumerable<KeyValuePair<string, object>>
	{
		bool TryGetValue(string key, out object value);

		event EventHandler<ResourcesChangedEventArgs> ValuesChanged;
	}
}