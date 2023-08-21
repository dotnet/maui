// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		protected override UINavigationBar CreatePlatformElement()
		{
			throw new System.NotImplementedException();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}
