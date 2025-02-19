using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TabbedPage)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42364, "ListView item's contextual action menu not being closed upon swiping a TabbedPage in AppCompat")]
	public class Bugzilla42354 : TestTabbedPage
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

			Children.Add(new ContentPage
			{
				Title = "Page One",
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
				}
			});

			Children.Add(new ContentPage { Title = "Page Two" });
		}
	}
}