using System;
using System.Collections.Generic;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class MyApp : MauiApp
	{
		// IAppState state
		public override IWindow CreateWindow(IActivationState state)
		{
			Forms.Init(state);
			return Services.GetRequiredService<IWindow>();
		}
	}
}