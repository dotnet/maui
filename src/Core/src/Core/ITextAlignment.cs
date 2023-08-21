// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to align Text.
	/// </summary>
	public interface ITextAlignment
	{
		/// <summary>
		/// Gets the horizontal text alignment.
		/// </summary>
		TextAlignment HorizontalTextAlignment { get; }

		/// <summary>
		/// Gets the vertical text alignment.
		/// </summary>
		TextAlignment VerticalTextAlignment { get; }
	}
}