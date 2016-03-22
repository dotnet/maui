using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	internal class ResourcesChangedEventArgs : EventArgs
	{
		public ResourcesChangedEventArgs(IEnumerable<KeyValuePair<string, object>> values)
		{
			Values = values;
		}

		public IEnumerable<KeyValuePair<string, object>> Values { get; private set; }
	}
}