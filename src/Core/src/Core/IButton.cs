// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a <see cref="IView"/> that reacts to touch events.
	/// </summary>
	public interface IButton : IView, IPadding, IButtonStroke
	{
		/// <summary>
		/// Occurs when the button is pressed.
		/// </summary>
		void Pressed();

		/// <summary>
		/// Occurs when the button is released.
		/// </summary>
		void Released();

		/// <summary>
		/// Occurs when the button is clicked/tapped.
		/// </summary>
		void Clicked();
	}
}