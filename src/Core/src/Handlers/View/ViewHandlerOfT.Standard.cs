// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView>
	{
		public override void PlatformArrange(Rect rect)
		{

		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
			=> Size.Zero;

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}