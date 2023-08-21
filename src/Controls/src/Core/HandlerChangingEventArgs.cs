// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public class HandlerChangingEventArgs
	{
		public IElementHandler NewHandler { get; }
		public IElementHandler OldHandler { get; }

		public HandlerChangingEventArgs(IElementHandler oldHandler, IElementHandler newHandler)
		{
			NewHandler = newHandler;
			OldHandler = oldHandler;
		}
	}
}
