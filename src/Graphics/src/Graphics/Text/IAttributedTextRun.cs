// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Text
{
	public interface IAttributedTextRun
	{
		int Start { get; }
		int Length { get; }

		ITextAttributes Attributes { get; }
	}
}
