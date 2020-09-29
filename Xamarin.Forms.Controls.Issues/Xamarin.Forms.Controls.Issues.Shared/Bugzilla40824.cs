using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40824, "ListView item's contextual action menu not being closed upon navigation in AppCompat")]
	public class Bugzilla40824 : TestContentPage
	{
		protected override void Init()
		{
			var list = new ListView
			{
				ItemsSource = new List<string>
				{
					"Cat",
					"Dog",
					"Rat"
				},
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, ".");
					cell.ContextActions.Add(new MenuItem
					{
						Text = "Action",
						IconImageSource = "icon",
						IsDestructive = true,
						Command = new Command(() => DisplayAlert("TITLE", "Context action invoked", "Ok")),
					});
					return cell;
				}),
			};

			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Go to next page",
						Command = new Command(() => Navigation.PushAsync(new ContentPage { Title = "Next Page", Content = new Label { Text = "Here" } }))
					},
					list
				}
			};
		}
	}
}