// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class WindowOverlayTappedEventArgs : EventArgs
	{
		public WindowOverlayTappedEventArgs(Point point, IList<IVisualTreeElement> elements, IList<IWindowOverlayElement> overlayElements)
		{
			Point = point;
			VisualTreeElements = elements;
			WindowOverlayElements = overlayElements;
		}

		public IList<IVisualTreeElement> VisualTreeElements { get; }

		public IList<IWindowOverlayElement> WindowOverlayElements { get; }

		public Point Point { get; }
	}
}