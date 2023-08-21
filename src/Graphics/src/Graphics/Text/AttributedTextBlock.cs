// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Text
{
	public class AttributedTextBlock
	{
		public string Text { get; }
		public ITextAttributes Attributes { get; }

		public AttributedTextBlock(string text, ITextAttributes attributes)
		{
			Text = text;
			Attributes = attributes;
		}

		public override string ToString()
		{
			return $"[AttributedTextBlock: Text={Text}, Attributes={Attributes}]";
		}
	}
}
