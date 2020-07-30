using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10830, "[Bug] [Fatal] [Android] CarouselView Inside Expander Causes Crashing", PlatformAffected.Android)]
	public class Issue10830 : TestContentPage
	{
		public Issue10830()
		{
			Title = "Issue 10830";

			Device.SetFlags(new List<string> { ExperimentalFlags.ExpanderExperimental });

			var layout = new Grid();

			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Expand the Expander and swipe the CarouselView. Without any crash, the test has passed."
			};

			var expander = new Expander();

			var expanderHeader = new Grid
			{
				BackgroundColor = Color.Red,
				HeightRequest = 50
			};

			var label = new Label
			{
				Text = "Success"
			};

			expanderHeader.Children.Add(label);

			expander.Header = expanderHeader;

			var carouselView = new CarouselView
			{
				BackgroundColor = Color.Green,
				HeightRequest = 200
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			carouselView.ItemTemplate = new DataTemplate(() =>
			{
				var template = new Grid
				{
					BackgroundColor = Color.Red,
					Padding = 24
				};

				var grid = new Grid
				{
					BackgroundColor = Color.Lime
				};

				template.Children.Add(grid);

				return template;
			});

			expander.Content = carouselView;

			layout.Children.Add(instructions);
			layout.Children.Add(expander);

			Content = layout;
		}

		public ObservableCollection<int> Items { get; } = new ObservableCollection<int>();

		protected override void Init()
		{
			for (int i = 0; i < 10; i++)
				Items.Add(i);

			BindingContext = this;
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue10830Test() 
		{
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}
