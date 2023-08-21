// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UIKit;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial interface IFontManager
	{
		/// <summary>
		/// Gets the default font for the operating system.
		/// </summary>
		UIFont DefaultFont { get; }

		/// <summary>
		/// Retrieves the platform equivalent <see cref="UIFont"/> for an abstract <see cref="Font"/> object.
		/// </summary>
		/// <param name="font">The abstract font representation to get the platform equivalent for.</param>
		/// <param name="defaultFontSize">The default font size to use for this font if no size is specified in <paramref name="font"/>.</param>
		/// <returns>The <see cref="UIFont"/> object representing the font as provided in <paramref name="font"/>.</returns>
		UIFont GetFont(Font font, double defaultFontSize = 0);
	}
}