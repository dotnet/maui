// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSeparatorHandler
	{
		protected override MenuFlyoutSeparator CreatePlatformElement()
		{
			return new MenuFlyoutSeparator();
		}
	}
}
