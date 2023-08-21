// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ExpressionSearch.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.ExpressionSearch']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/ExpressionSearch.xml" path="//Member[@MemberName='Default']/Docs/*" />
		public static IExpressionSearch Default { get; set; }
	}
}