// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface ILoopItemsViewSource : IItemsViewSource
	{
		bool Loop { get; set; }

		int LoopCount { get; }
	}
}