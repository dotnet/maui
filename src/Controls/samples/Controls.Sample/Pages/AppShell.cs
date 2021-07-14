using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;


namespace Maui.Controls.Sample.Pages
{
	public class AppShell : Shell
	{
		public AppShell(IServiceProvider services, MainViewModel viewModel)
		{
			Items.Add(new FlyoutItem() { Title = "Flyout Item 1", Items = { new MainPage(services, viewModel), new SemanticsPage() } });
			Items.Add(new FlyoutItem() { Title = "Flyout Item 2", Items = { new MainPage(services, viewModel), new SemanticsPage() } });
		}
	}
}
