// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.RootNavigationView;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.DrawerView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IFlyoutViewHandler : IViewHandler
	{
		new IFlyoutView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}