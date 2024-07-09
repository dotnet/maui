using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9794, "[iOS] Tabbar Disappears with linker",
		PlatformAffected.iOS)]
	public class Issue9794 : TestShell
	{
		protected override void Init()
		{
			var page1 = AddBottomTab("tab1");
			AddBottomTab("tab2");

			page1.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Push a page, click back button, and then click between the tabs. If the tab bar disappears the test has failed.",
					},
					new Button()
					{
						Text = "Push Page",
						AutomationId = "GoForward",
						Command = new Command(async () =>
						{
							await Navigation.PushAsync(new Issue9794Modal());
						})
					}
				}
			};
		}

		public class Issue9794Modal : ContentPage
		{
			public Issue9794Modal()
			{
				Shell.SetTabBarIsVisible(this, false);
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Click Back Button"
						}
					}
				};
			}
		}
	}
}
