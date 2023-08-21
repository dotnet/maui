// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	internal class ChildCollectionChangedEventArgs : EventArgs
	{
		public ChildCollectionChangedEventArgs(NotifyCollectionChangedEventArgs args)
		{
			Args = args;
		}

		public NotifyCollectionChangedEventArgs Args { get; private set; }
	}
}