// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public interface ITextAttributes
	{
		IFont Font { get; set; }

		float FontSize { get; set; }

		float Margin { get; set; }

		Color TextFontColor { get; set; }

		HorizontalAlignment HorizontalAlignment { get; set; }

		VerticalAlignment VerticalAlignment { get; set; }
	}
}
