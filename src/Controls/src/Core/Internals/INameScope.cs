// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.ComponentModel;
using System.Xml;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INameScope
	{
		/// <summary>For internal use by .NET MAUI.</summary>
		object FindByName(string name);

		/// <summary>For internal use by .NET MAUI.</summary>
		void RegisterName(string name, object scopedElement);

		/// <summary>For internal use by .NET MAUI.</summary>
		void UnregisterName(string name);
	}
}