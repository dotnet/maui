// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IScrollViewController : ILayoutController
	{
		Point GetScrollPositionForElement(VisualElement item, ScrollToPosition position);

		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		void SendScrollFinished();

		void SetScrolledPosition(double x, double y);

		Rect LayoutAreaOverride { get; set; }
	}
}