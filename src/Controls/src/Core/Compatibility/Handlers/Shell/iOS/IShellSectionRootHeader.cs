// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSectionRootHeader : IDisposable
	{
		UIViewController ViewController { get; }
		ShellSection ShellSection { get; set; }
	}
}