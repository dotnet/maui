﻿using Foundation;
using Microsoft.Maui;

namespace Maui.Controls.Sample.SingleProject
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}