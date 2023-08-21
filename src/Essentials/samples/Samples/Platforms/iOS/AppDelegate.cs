// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Samples.iOS
{
	[Register(nameof(AppDelegate))]
	public partial class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}