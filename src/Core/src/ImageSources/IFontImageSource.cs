// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IFontImageSource : IImageSource
	{
		Color Color { get; }

		Font Font { get; }

		string Glyph { get; }
	}
}