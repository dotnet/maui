// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public abstract class Paint
	{
		public Color BackgroundColor { get; set; }

		public Color ForegroundColor { get; set; }

		public virtual bool IsTransparent { get; }
	}
}