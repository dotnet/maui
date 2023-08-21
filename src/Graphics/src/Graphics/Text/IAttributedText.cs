// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
	public interface IAttributedText
	{
		string Text { get; }
		IReadOnlyList<IAttributedTextRun> Runs { get; }
	}
}
