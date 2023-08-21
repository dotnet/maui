// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to accept multi-line input.
	/// </summary>
	public interface IEditor : IView, ITextInput, ITextStyle, ITextAlignment
	{
		/// <summary>
		/// Occurs when the user finalizes the text in an editor with the return key.
		/// </summary>
		void Completed();
	}
}
