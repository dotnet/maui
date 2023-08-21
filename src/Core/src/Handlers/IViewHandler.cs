// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IViewHandler : IElementHandler
	{
		bool HasContainer { get; set; }

		object? ContainerView { get; }

		new IView? VirtualView { get; }

		Size GetDesiredSize(double widthConstraint, double heightConstraint);

		void PlatformArrange(Rect frame);
	}
}