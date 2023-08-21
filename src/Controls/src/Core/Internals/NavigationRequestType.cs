// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.NavigationRequestType']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum NavigationRequestType
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='Unknown']/Docs/*" />
		Unknown = 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='Push']/Docs/*" />
		Push,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='Pop']/Docs/*" />
		Pop,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='PopToRoot']/Docs/*" />
		PopToRoot,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='Insert']/Docs/*" />
		Insert,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestType.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		Remove,
	}
}