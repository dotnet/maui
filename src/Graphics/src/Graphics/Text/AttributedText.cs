// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
	public class AttributedText : AbstractAttributedText
	{
		public AttributedText(
			string text,
			IReadOnlyList<IAttributedTextRun> runs,
			bool optimal = false)
		{
			Text = text;
			Runs = runs;
			Optimal = optimal;
		}

		public override string Text { get; }

		public override IReadOnlyList<IAttributedTextRun> Runs { get; }
	}
}
