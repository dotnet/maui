using System;
using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3624, "Layout Compression causes the app to crash when scrolling a ListView with ListViewCachingStrategy.RetainElement")]
	public class Issue3624 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "With Compression (type 1) --- Crash while scrolling",
						Command = new Command(() => {
							TestPage.ShouldUseCompressedLayout = true;
							Navigation.PushAsync(new TestPage());
						})
					},
					new Button
					{
						Text = "Without Compression (type 1) --- Scrolls fine",
						Command = new Command(() => {
							TestPage.ShouldUseCompressedLayout = false;
							Navigation.PushAsync(new TestPage());
						})
					},
					new Button
					{
						Text = "With Compression (type 2) --- Crash while scrolling",
						Command = new Command(() => {
							TestPage.ShouldUseCompressedLayout = true;
							Navigation.PushAsync(new TestPage(useType2: true));
						})
					},
					new Button
					{
						Text = "Without Compression (type 2) --- Scrolls fine",
						Command = new Command(() => {
							TestPage.ShouldUseCompressedLayout = false;
							Navigation.PushAsync(new TestPage(useType2: true));
						})
					}
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class TestPage : ContentPage
		{
			public static bool ShouldUseCompressedLayout = false;
			public ObservableCollection<VeggieViewModel> veggies { get; set; }

			public TestPage(bool useType2 = false)
			{
				veggies = new ObservableCollection<VeggieViewModel>();
				ListView listView = new ListView
				{
					RowHeight = 60
				};
				Title = ShouldUseCompressedLayout ? "Scroll & crash" : "Scrolls fine";
				listView.ItemTemplate = new DataTemplate(useType2 ? typeof(TestCell2) : typeof(TestCell1));

				for (int i = 0; i < 1000; i++)
				{
					switch (i % 3)
					{
						case 0:
							veggies.Add(new VeggieViewModel { Name = $"#{i} Tomato" });
							break;
						case 1:
							veggies.Add(new VeggieViewModel { Name = $"#{i} Romaine Lettuce" });
							break;
						case 2:
							veggies.Add(new VeggieViewModel { Name = $"#{i} Zucchini" });
							break;
					}
				}

				listView.ItemsSource = veggies;
				Content = listView;
			}
		}

		[Preserve(AllMembers = true)]
		public class TestCell1 : ViewCell
		{
			public TestCell1()
			{
				var nameLabel = new Label();
				var verticaLayout = new StackLayout();
				Forms.CompressedLayout.SetIsHeadless(verticaLayout, TestPage.ShouldUseCompressedLayout);
				var horizontalLayout = new StackLayout() { BackgroundColor = Color.Olive };

				nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));

				horizontalLayout.Orientation = StackOrientation.Horizontal;
				horizontalLayout.HorizontalOptions = LayoutOptions.Fill;

				verticaLayout.Children.Add(nameLabel);
				horizontalLayout.Children.Add(verticaLayout);

				View = horizontalLayout;
			}
		}

		[Preserve(AllMembers = true)]
		public partial class TestCell2 : ViewCell
		{
			public TestCell2()
			{
				var layout = new AbsoluteLayout();
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				var stack = new StackLayout
				{
					Children = { label }
				};
				var grid = new Grid()
				{
					ColumnDefinitions = new ColumnDefinitionCollection {
						new ColumnDefinition { Width = GridLength.Star }
					}
				};
				Forms.CompressedLayout.SetIsHeadless(stack, true);
				grid.AddChild(stack, 0, 0);
				layout.Children.Add(grid, new Rectangle(0,0,1,1), AbsoluteLayoutFlags.All);

				View = layout;
				Forms.CompressedLayout.SetIsHeadless(stack, TestPage.ShouldUseCompressedLayout);
			}
		}

		[Preserve(AllMembers = true)]
		public class VeggieViewModel
		{
			public string Name { get; set; }
		}
	}
}
