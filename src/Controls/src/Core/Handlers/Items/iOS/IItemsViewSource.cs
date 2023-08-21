// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IItemsViewSource : IDisposable
	{
		int ItemCount { get; }
		int ItemCountInGroup(nint group);
		int GroupCount { get; }
		object this[Foundation.NSIndexPath indexPath] { get; }
		object Group(Foundation.NSIndexPath indexPath);
		IItemsViewSource GroupItemsViewSource(Foundation.NSIndexPath indexPath);
		Foundation.NSIndexPath GetIndexForItem(object item);
	}
}