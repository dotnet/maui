using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5765, "[Frame, CollectionView, Android]The Label.Text is invisible on Android if DataTemplate have frame as layout",
		PlatformAffected.Android)]
	class Issue5765 : TestNavigationPage
	{
		const string Target = "FirstLabel";

		protected override void Init()
		{
			PushAsync(CreateRoot());
		}

		Frame CreateFrame()
		{
			var frame = new Frame() { CornerRadius = 10, BackgroundColor = Colors.SeaGreen };

			var flexLayout = new FlexLayout()
			{
				Direction = FlexDirection.Row,
				JustifyContent = FlexJustify.SpaceBetween,
				AlignItems = FlexAlignItems.Stretch
			};

			var label1 = new Label { Text = "First Label", AutomationId = Target, HeightRequest = 100 };
			var label2 = new Label { Text = "Second Label" };

			flexLayout.Children.Add(label1);
			flexLayout.Children.Add(label2);

			frame.Content = flexLayout;

			return frame;
		}

		Page CreateRoot()
		{
			var page = new ContentPage() { Title = "Issue5765" };

			var cv = new CollectionView();

			cv.ItemTemplate = new DataTemplate(() =>
			{
				return CreateFrame();
			});

			cv.ItemsSource = new List<string> { "one", "two", "three" };

			page.Content = cv;

			return page;
		}
	}
}
