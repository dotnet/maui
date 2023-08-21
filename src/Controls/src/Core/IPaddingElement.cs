// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPaddingElement
	{
		//note to implementor: implement this property publicly
		Thickness Padding { get; }

		//note to implementor: but implement this method explicitly
		void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue);
		Thickness PaddingDefaultValueCreator();
	}
}