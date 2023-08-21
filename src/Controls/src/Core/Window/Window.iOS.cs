// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UIWindow NativeWindow =>
			(Handler?.PlatformView as UIWindow) ?? throw new InvalidOperationException("Window should have a UIWindow set.");
	}
}