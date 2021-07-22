using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class AppShell : Shell
	{
		public AppShell(IServiceProvider services, MainViewModel viewModel)
		{
			Items.Add(new FlyoutItem() { Title = "Flyout Item 1", Items = { new SemanticsPage(), new ButtonPage(), } });
			Items.Add(new FlyoutItem() { Title = "Flyout Item 2", Items = { new ButtonPage(), new SemanticsPage() } });
			Items.Add(new ShellSection() { Title = "Flyout Item 3", 
				Items = {
					new ShellContent()
					{
						Title = "Semantics Page",
						Content = new SemanticsPage() { Title = "Semantics Page" }
					},
					new ShellContent()
					{
						Title = "Button Page",
						Content = new ButtonPage() { Title = "Button Page" }
					},
				}});

		}
	}
}
