using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29363, "PushModal followed immediate by PopModal crashes")]
	public class Bugzilla29363 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

			Button modal = new Button
			{
				Text = "Modal Push Pop Test",
				FontAttributes = FontAttributes.Bold,
				FontSize = 25,
				HorizontalOptions = LayoutOptions.Center
			};
			modal.Clicked += async (object sender, EventArgs e) =>
			{
				var page = new ContentPage() { BackgroundColor = Colors.Red };

				await Navigation.PushModalAsync(page);

				await Navigation.PopModalAsync(true);
			};

			layout.Children.Add(modal);
			Content = layout;
		}
	}
}
