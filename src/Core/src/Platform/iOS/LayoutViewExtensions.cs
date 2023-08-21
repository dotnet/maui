// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Platform
{
	public static class LayoutViewExtensions
	{
		public static void UpdateClipsToBounds(this LayoutView layoutView, ILayout layout)
		{
			layoutView.ClipsToBounds = layout.ClipsToBounds;
		}
	}
}