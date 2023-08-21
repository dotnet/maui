// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		partial void HandlerChangedPartial()
		{
			this.AddOrRemoveControlsAccessibilityDelegate();
		}
	}
}
