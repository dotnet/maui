using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12888,
		"Fix timing on iOS Toolbar",
		PlatformAffected.iOS)]
	public class Issue12888 : Shell
	{
		const string Success = "Success";
		const string Show = "Show";

		public Issue12888()
		{
			var page1 = CreatePage("Item 1");
			var page2 = CreatePage("Item 2");

			var shellTitleView =
				new HorizontalStackLayout()
				{
					new Editor() { AutomationId = "Success", WidthRequest = 100},
					new Label() { Text = "Title View" },
				};

			Shell.SetTitleView(this, shellTitleView);
			this.Items.Add(new TabBar()
			{
				Items =
				{
					new ShellContent()
					{
						Route = "Item1",
						Content = page1,
						Title = "Item 1"
					},
					new ShellContent()
					{
						Route = "Item2",
						Content = page2,
						Title = "Item 2"
					},
				}
			});
		}

		ContentPage CreatePage(string title)
		{
			return new ContentPage()
			{
				Title = title,
				Content = new StackLayout()
				{
					Children =
					{
						new Label() { Text = "Click on different buttons to navigate around shell. The TitleView should remain in place as you navigate" },
						new Button() { Text = "Go to Item 1", Command = new Command(async () => await this.GoToAsync("//Item1")), AutomationId = "GoToItem1" },
						new Button() { Text = "Go to Item 2", Command = new Command(async () => await this.GoToAsync("//Item2")), AutomationId = "GoToItem2" },
						new Button() { Text = "Push New Page",
							Command =
								new Command(async () => await this.Navigation.PushAsync(CreatePage($"Pushed Page: {DateTime.Now.ToLongTimeString()}"))),
									AutomationId = "PushPage" },
						new Button() { Text = "Pop Page", Command = new Command(async () => await this.Navigation.PopAsync()), AutomationId = "PopPage" },
					}
				}
			};
		}
	}
}