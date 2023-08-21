// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls
{
	internal class ToolbarItemEventArgs : EventArgs
	{
		public ToolbarItemEventArgs(ToolbarItem item)
		{
			ToolbarItem = item;
		}

		public ToolbarItem ToolbarItem { get; private set; }
	}
}