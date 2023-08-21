// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BindablePropertyKey.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindablePropertyKey']/Docs/*" />
	public sealed class BindablePropertyKey
	{
		internal BindablePropertyKey(BindableProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			BindableProperty = property;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindablePropertyKey.xml" path="//Member[@MemberName='BindableProperty']/Docs/*" />
		public BindableProperty BindableProperty { get; private set; }
	}
}