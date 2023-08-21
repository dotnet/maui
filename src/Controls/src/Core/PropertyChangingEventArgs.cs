// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PropertyChangingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.PropertyChangingEventArgs']/Docs/*" />
	public class PropertyChangingEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/PropertyChangingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public PropertyChangingEventArgs(string propertyName)
		{
			PropertyName = propertyName;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PropertyChangingEventArgs.xml" path="//Member[@MemberName='PropertyName']/Docs/*" />
		public virtual string PropertyName { get; }
	}
}