// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Markdig.Renderers;
using Markdig.Syntax;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public abstract class AttributedTextObjectRenderer<T>
		: MarkdownObjectRenderer<AttributedTextRenderer, T> where T : MarkdownObject
	{
	}
}
