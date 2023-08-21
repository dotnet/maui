// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{

	public interface IShellNavBarAppearanceTracker : IDisposable
	{
		void ResetAppearance(UINavigationController controller);
		void SetAppearance(UINavigationController controller, ShellAppearance appearance);
		void UpdateLayout(UINavigationController controller);
		void SetHasShadow(UINavigationController controller, bool hasShadow);
	}
}