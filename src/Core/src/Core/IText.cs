// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Text.
	/// </summary>
	public interface IText : ITextStyle
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }
	}
}