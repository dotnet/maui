using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3507, "[UWP] Scrollview with null content crashes on UWP",
		PlatformAffected.UWP)]
	public class Issue3507 : TestContentPage
	{
		Label label;
		ScrollView scrollView;
		protected override void Init()
		{
			scrollView = new ScrollView();
			label = new Label();

			Content = new StackLayout()
			{
				Children =
				{
					label,
					scrollView
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(500);
			scrollView.Content = new StackLayout();
			await Task.Delay(500);
			scrollView.Content = null;
			await Task.Delay(500);
			label.Text = "Success";
		}
	}
}
