using System;
using System.Collections.Generic;
using System.Linq;
using Maui.Controls.Sample.Controls;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.LifecycleEvents;
using Debug = System.Diagnostics.Debug;


namespace Maui.Controls.Sample.Pages
{
	public class AppShell : Shell
	{
		public AppShell(IServiceProvider services, MainPageViewModel viewModel)
		{
			Items.Add(new FlyoutItem() { Title = "Flyout Item 1", Items = { new MainPage(services, viewModel), new SemanticsPage() } });
			Items.Add(new FlyoutItem() { Title = "Flyout Item 2", Items = { new MainPage(services, viewModel), new SemanticsPage() } });
		}
	}
}
