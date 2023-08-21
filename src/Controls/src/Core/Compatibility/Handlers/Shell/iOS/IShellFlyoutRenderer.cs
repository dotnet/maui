// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		UIView View { get; }

		void AttachFlyout(IShellContext context, UIViewController content);
	}
}