#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ResourcesChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.ResourcesChangedEventArgs']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ResourcesChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ResourcesChangedEventArgs.xml" path="//Member[@MemberName='StyleSheets']/Docs/*" />
		public static readonly ResourcesChangedEventArgs StyleSheets = new ResourcesChangedEventArgs(null);

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ResourcesChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ResourcesChangedEventArgs(IEnumerable<KeyValuePair<string, object>> values)
		{
			Values = values;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ResourcesChangedEventArgs.xml" path="//Member[@MemberName='Values']/Docs/*" />
		public IEnumerable<KeyValuePair<string, object>> Values { get; private set; }
	}
}