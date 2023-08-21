// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/CheckedChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckedChangedEventArgs']/Docs/*" />
	public class CheckedChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/CheckedChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CheckedChangedEventArgs(bool value)
		{
			Value = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/CheckedChangedEventArgs.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public bool Value { get; private set; }
	}
}