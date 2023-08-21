// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	public partial class MenuFlyoutSeparatorHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			var menuContainer =
				UIMenu.Create(string.Empty,
					image: null,
					UIMenuIdentifier.None,
					UIMenuOptions.DisplayInline,
					children: Array.Empty<UIMenuElement>());

			return menuContainer;
		}
	}
}
