// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface IMenuFlyoutSeparatorHandler : IElementHandler
	{
		new PlatformView PlatformView { get; }
		new IMenuFlyoutSeparator VirtualView { get; }
	}
}
