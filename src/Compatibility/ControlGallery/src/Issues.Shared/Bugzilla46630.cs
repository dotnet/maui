﻿using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 46630, "[Xamarin.Forms, Android] Context menu of the Editor control is not working in the ListView", PlatformAffected.Android)]
	public class Bugzilla46630 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				HasUnevenRows = true,
				ItemsSource = new List<int> { 0 },
				ItemTemplate = new DataTemplate(() => new ViewCell
				{
					Height = 300,
					ContextActions =
					{
						new MenuItem {Text = "Action1"},
						new MenuItem {Text = "Action2"}
					},
					View = new StackLayout
					{
						Orientation = StackOrientation.Vertical,
						Spacing = 10,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Padding = 10,
						Children =
						{
							new Label { HeightRequest = 50, BackgroundColor = Colors.Coral, Text = "Long click each cell. Input views should not display context actions."},
							new Editor { HeightRequest = 50, BackgroundColor = Colors.Bisque, Text = "Editor"},
							new Entry { HeightRequest = 50, BackgroundColor = Colors.Aqua, Text = "Entry"},
							new SearchBar { HeightRequest = 50, BackgroundColor = Colors.CornflowerBlue, Text = "SearchBar"},
							new Grid { HeightRequest = 50, BackgroundColor = Colors.PaleVioletRed}
						}
					}
				})
			};
		}
	}
}