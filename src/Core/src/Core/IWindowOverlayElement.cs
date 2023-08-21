// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Element drawn on top of IWindowOverlay.
	/// </summary>
	public interface IWindowOverlayElement : IDrawable
	{
		/// <summary>
		/// Gets a boolean for if the given point contained within the window overlay element.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>Boolean if the point is contained within the element.</returns>
		bool Contains(Point point);
	}
}