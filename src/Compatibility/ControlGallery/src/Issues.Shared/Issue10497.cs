using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10497, "[Bug] Controls inside CollectionView might flash scrollbar while they're not scrollable", PlatformAffected.Android)]
	public class Issue10497 : TestContentPage
	{
		public Issue10497()
		{
			Title = "Issue 10497";

			var layout = new StackLayout();

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If loading the page you don't see the scrollbar in each CollectionView Item, the test has passed."
			};

			var scrollBarVisibilityPicker = new Picker
			{
				Title = "VerticalScrollBarVisibility",
				ItemsSource = new List<string>
				{
					"Default",
					"Always",
					"Never"
				},
				SelectedIndex = 0
			};

			var collectionView = new CollectionView
			{
				Margin = new Thickness(0, 0, 50, 0)
			};

			collectionView.ItemsSource = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5",
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				"Item 7",
				"Item 8",
				"Item 9",
				"Item 10"
			};

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HeightRequest = 60,
					FontSize = 75
				};

				label.SetBinding(Label.TextProperty, ".");

				return label;
			});

			layout.Children.Add(instructions);
			layout.Children.Add(scrollBarVisibilityPicker);
			layout.Children.Add(collectionView);

			Content = layout;

			scrollBarVisibilityPicker.SelectedIndexChanged += (sender, args) =>
			{
				switch (scrollBarVisibilityPicker.SelectedIndex)
				{
					case 0:
						collectionView.VerticalScrollBarVisibility = ScrollBarVisibility.Default;
						break;
					case 1:
						collectionView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
						break;
					case 2:
						collectionView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
						break;
				}
			};
		}

		protected override void Init()
		{

		}
	}
}
