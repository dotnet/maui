// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Markdig.Syntax.Inlines;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public class LineBreakInlineRenderer : AttributedTextObjectRenderer<LineBreakInline>
	{
		protected override void Write(AttributedTextRenderer renderer, LineBreakInline obj)
		{
			renderer.WriteLine();
		}
	}
}
