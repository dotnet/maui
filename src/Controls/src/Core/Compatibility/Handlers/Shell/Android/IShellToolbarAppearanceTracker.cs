// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using AndroidX.AppCompat.Widget;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellToolbarAppearanceTracker : IDisposable
	{
		void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance);
		void ResetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker);
	}
}