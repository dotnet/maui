// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class BoxViewHandler : ShapeViewHandler
	{
		public override bool NeedsContainer =>
			VirtualView?.Clip is not null ||
			VirtualView?.Shadow is not null;
	}
}