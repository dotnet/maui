using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;

#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33248, "Entry.Completed calling Editor.Focus() inserts new line to the focused Editor in iOS", PlatformAffected.iOS)]
	public class Bugzilla33248 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				BackgroundColor = Color.Yellow,
				HeightRequest = 300
			};
			var entry = new Entry
			{
				BackgroundColor = Color.Red,
				HeightRequest = 100
			};

			entry.Completed += (sender, e) => editor.Focus();

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Start,
				Children =
				{
					new Label
					{
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Entry:"
					},
					entry,
					new Label
					{
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Editor:"
					},
					editor
				}
			};
		}
	}
}
