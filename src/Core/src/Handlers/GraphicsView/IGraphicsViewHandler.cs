// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if __IOS__ || MACCATALYST || MONOANDROID || WINDOWS || TIZEN
using PlatformView = Microsoft.Maui.Platform.PlatformTouchGraphicsView;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IGraphicsViewHandler : IViewHandler
	{
		new IGraphicsView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}