using System;
using System.Threading.Tasks;
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
	[Category(UITestCategories.RefreshView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8198, "ScrollView at CollectionView at RefreshView always leads to Pull-To-Refresh", PlatformAffected.Android)]
	public class Issue8198 : TestContentPage
	{
		RefreshView _refreshView;
		Command _refreshCommand;

		protected override void Init()
		{
			Title = "Issue 8198";

			var layout = new StackLayout();

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Scroll the CollectionView to end, lift finger off screen, and then try to scroll up again. If the Refresh Indicator does not appear until it reaches the top, the test has passed."
			};

			_refreshCommand = new Command(async (parameter) =>
			{
				if (!_refreshView.IsRefreshing)
				{
					throw new Exception("IsRefreshing should be true when command executes");
				}

				if (parameter != null && !(bool)parameter)
				{
					throw new Exception("Refresh command incorrectly firing with disabled parameter");
				}

				await Task.Delay(2000);
				_refreshView.IsRefreshing = false;
			});

			_refreshView = new RefreshView
			{
				Command = _refreshCommand
			};

			var collectionView = new CollectionView
			{
				ItemTemplate = GetDataTemplate(),
				ItemsSource = new string[]
				{
					"Item 1",
					"Item 2",
					"Item 3",
					"Item 4",
					"Item 5",
					"item 6",
					"Item 7",
					"Item 8",
					"Item 9",
					"Item 10",
	 				"Item 11",
					"item 12",
					"Item 13",
					"Item 14",
					"Item 15",
					"Item 16",
					"Item 17",
					"Item 18",
					"Item 19",
					"Item 20"
				}
			};

			_refreshView.Content = collectionView;

			layout.Children.Add(instructions);
			layout.Children.Add(_refreshView);

			Content = layout;
		}

		DataTemplate GetDataTemplate()
		{
			var template = new DataTemplate(() =>
			{
				var scroll = new ScrollView();
				var grid = new Grid();
				grid.RowDefinitions.Add(new RowDefinition() { Height = 40 });

				var cell = new Label();
				cell.SetBinding(Label.TextProperty, ".");
				cell.FontSize = 20;
				cell.BackgroundColor = Color.LightBlue;
				grid.Children.Add(cell, 0, 0);

				scroll.Content = grid;
				return scroll;
			});
			return template;
		}
	}
}