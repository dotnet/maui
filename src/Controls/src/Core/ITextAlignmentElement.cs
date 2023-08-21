// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	interface ITextAlignmentElement
	{
		//note to implementor: implement the properties publicly
		TextAlignment HorizontalTextAlignment { get; }

		TextAlignment VerticalTextAlignment { get; }

		//note to implementor: but implement the methods explicitly
		void OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue);
	}
}