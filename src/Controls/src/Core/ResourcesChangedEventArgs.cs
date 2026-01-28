#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Event arguments for resource dictionary changes.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ResourcesChangedEventArgs : EventArgs
	{
		/// <summary>Singleton instance used for stylesheet changes.</summary>
		public static readonly ResourcesChangedEventArgs StyleSheets = new ResourcesChangedEventArgs(null);

		/// <summary>Creates a new <see cref="ResourcesChangedEventArgs"/> with the specified values.</summary>
		/// <param name="values">The changed resource values.</param>
		public ResourcesChangedEventArgs(IEnumerable<KeyValuePair<string, object>> values)
		{
			Values = values;
		}

		/// <summary>Gets the changed resource values.</summary>
		public IEnumerable<KeyValuePair<string, object>> Values { get; private set; }
	}
}