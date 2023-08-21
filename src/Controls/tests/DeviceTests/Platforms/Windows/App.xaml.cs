// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		public App() => InitializeComponent();

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
