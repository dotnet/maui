// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that provides a toggled value.
	/// </summary>
	public interface IRadioButton : IView, ITextStyle, IContentView, IButtonStroke
	{
		/// <summary>
		/// Gets or sets a Boolean value that indicates whether this RadioButton is checked.
		/// </summary>
		bool IsChecked { get; set; }
	}
}