using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 465, "Change in Navigation.PushModal", PlatformAffected.All)]
	public class Issue465 : TestTabbedPage
	{
		protected override async void Init()
		{
			Children.Add(
				new ContentPage
				{
					Content = new Label
					{
						AutomationId = "Popppppped",
						Text = "I was popppppped"
					}
				}
			);

			await Navigation.PushModalAsync(new ModalPage());
		}

		[Preserve(AllMembers = true)]
		public class ModalPage : ContentPage
		{
			public ModalPage()
			{
				var popButton = new Button
				{
					AutomationId = "PopPage",
					Text = "Pop this page"
				};
				popButton.Clicked += (s, e) => Navigation.PopModalAsync();

				Content = popButton;
			}
		}
	}
}
