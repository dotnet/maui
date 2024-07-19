using System;
using ObjCRuntime;
using UIKit;

using PlatformWindow = UIKit.UIWindow;

namespace Microsoft.Maui.Embedding;

internal partial class EmbeddedWindowHandler
{
	protected override void ConnectHandler(PlatformWindow platformView)
	{
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(PlatformWindow platformView)
	{
		base.DisconnectHandler(platformView);
	}
}
