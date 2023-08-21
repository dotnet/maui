// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Text
{
	public class AttributedTextRun : IAttributedTextRun
	{
		public AttributedTextRun(
			int start,
			int length,
			ITextAttributes attributes)
		{
			Start = start;
			Length = length;
			Attributes = attributes;
		}

		public int Start { get; }

		public int Length { get; }

		public ITextAttributes Attributes { get; }

		public override string ToString()
		{
			return $"[AttributedTextRun: Start={Start}, Length={Length}, Attributes={Attributes}]";
		}
	}
}
