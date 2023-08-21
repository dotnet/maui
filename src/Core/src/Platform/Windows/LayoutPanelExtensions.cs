// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.Maui.Platform
{
	public static class LayoutPanelExtensions
	{
		public static void UpdateClipsToBounds(this LayoutPanel layoutPanel, ILayout layout)
		{
			layoutPanel.ClipsToBounds = layout.ClipsToBounds;
			layoutPanel.InvalidateArrange();
		}
	}
}
