using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42519, "Text Truncation in UWP")]
	public class Bugzilla42519 : TestNavigationPage
	{
		public static readonly string LongLabelSingle =
			"longleftlabelthequickbrownfoxjumpedoverthelazydogsthequickbrownfoxjumpedoverthelazydogs";

		public static readonly string LongLabelWords =
			"long left label the quick brown fox jumped over the lazy dogs the quick brown fox jumped over the lazy dogs";

		protected override void Init()
		{
			PushAsync(Menu());
		}

		static ContentPage CreateContent(Type cellType)
		{
			return new ContentPage { Content = CreateListView(new DataTemplate(cellType)) };
		}

		static ListView CreateListView(DataTemplate template)
		{
			var items = new List<_42519Item>
			{
				new _42519Item
				{
					TitleLeft = LongLabelWords,
					TitleRight = "32522665",
					SubLeft = "LeftLabel",
					SubRight = "Long Right Label"
				},
				new _42519Item
				{
					TitleLeft = LongLabelSingle,
					TitleRight = "12552665222",
					SubLeft = "LeftLabel",
					SubRight = "Long Right Label"
				},
				new _42519Item
				{
					TitleLeft = LongLabelSingle,
					TitleRight = "225565365",
					SubLeft = "LeftLabel",
					SubRight = "Long Right Label"
				},
				new _42519Item
				{
					TitleLeft = LongLabelWords,
					TitleRight = "215565365",
					SubLeft = "LeftLabel",
					SubRight = "Long Right Label"
				}
			};

			return new ListView
			{
				HasUnevenRows = true,
				ItemTemplate = template,
				ItemsSource = items
			};
		}

		static ContentPage Menu()
		{
			var page = new ContentPage();

			var heading = new Label { Text = "Select an option below to see text tail truncation in various contexts." };

			if (Device.Idiom == TargetIdiom.Phone)
			{
				heading.Text += " Rotating the phone between portrait and landscape mode should not cause the ellipsis to disappear from truncated text.";
			}

			var labelButton = new Button { Text = "Single Label" };
			var gridButton = new Button { Text = "Single Grid" };
			var listWithLabelsButton = new Button { Text = "ListView with Label ViewCell" };
			var listWithGridsButton = new Button { Text = "ListView with Grid ViewCell" };

			labelButton.Clicked += (sender, args) =>
			{
				var content = new Label { Text = LongLabelSingle, LineBreakMode = LineBreakMode.TailTruncation };
				page.Navigation.PushAsync(new ContentPage { Content = content });
			};

			gridButton.Clicked += (sender, args) =>
			{
				var content = new ContentPage
				{
					Content = _42519CustomViewCellGrid.CreateGrid(),
					BindingContext = new _42519Item
					{
						TitleLeft = LongLabelSingle,
						TitleRight = "32522665",
						SubLeft = "LeftLabel",
						SubRight = "Long Right Label"
					}
				};
				page.Navigation.PushAsync(content);
			};

			listWithLabelsButton.Clicked +=
				(sender, args) => { page.Navigation.PushAsync(CreateContent(typeof(_42519CustomViewCellLabel))); };

			listWithGridsButton.Clicked +=
				(sender, args) => { page.Navigation.PushAsync(CreateContent(typeof(_42519CustomViewCellGrid))); };

			page.Content = new StackLayout
			{
				Children =
				{
					heading,
					labelButton,
					gridButton,
					listWithLabelsButton,
					listWithGridsButton
				}
			};

			return page;
		}

		[Preserve(AllMembers = true)]
		internal class _42519Item
		{
			public string SubLeft { get; set; }

			public string SubRight { get; set; }

			public string TitleLeft { get; set; }

			public string TitleRight { get; set; }
		}

		[Preserve(AllMembers = true)]
		internal class _42519CustomViewCellGrid : ViewCell
		{
			public _42519CustomViewCellGrid()
			{
				View = CreateGrid();
			}

			public static Grid CreateGrid()
			{
				var grid = new Grid
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					RowDefinitions =
					{
						new RowDefinition { Height = new GridLength(24, GridUnitType.Absolute) },
						new RowDefinition { Height = new GridLength(24, GridUnitType.Absolute) }
					},
					ColumnDefinitions =
					{
						new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
						new ColumnDefinition { Width = GridLength.Auto }
					},
					Padding = new Thickness(16, 12, 16, 12)
				};
				var leftLabel = new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				};
				leftLabel.SetBinding(Label.TextProperty, "TitleLeft");

				var rightLabel = new Label
				{
					FontSize = 20,
					HorizontalOptions = LayoutOptions.End
				};
				rightLabel.SetBinding(Label.TextProperty, "TitleRight");

				var subLeft = new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				};
				subLeft.SetBinding(Label.TextProperty, "SubLeft");

				var subRight = new Label
				{
					HorizontalOptions = LayoutOptions.End
				};
				subRight.SetBinding(Label.TextProperty, "SubRight");

				grid.Children.Add(leftLabel, 0, 0);
				grid.Children.Add(rightLabel, 1, 0);
				grid.Children.Add(subLeft, 0, 1);
				grid.Children.Add(subRight, 1, 1);

				return grid;
			}
		}

		[Preserve(AllMembers = true)]
		internal class _42519CustomViewCellLabel : ViewCell
		{
			public _42519CustomViewCellLabel()
			{
				var leftLabel = new Label
				{
					LineBreakMode = LineBreakMode.TailTruncation
				};

				leftLabel.SetBinding(Label.TextProperty, "TitleLeft");

				View = leftLabel;
			}
		}
	}
}
