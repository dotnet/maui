// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new UIView? PlatformView { get; }
		new UIView? ContainerView { get; }
		UIViewController? ViewController { get; }
	}
}