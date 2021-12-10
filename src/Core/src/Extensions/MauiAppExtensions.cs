using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	internal static class MauiAppExtensions
	{
		public static async void StartBackgroundServices(this MauiApp mauiApp)
		{
			await mauiApp.StartAsync();
		}
	}
}
