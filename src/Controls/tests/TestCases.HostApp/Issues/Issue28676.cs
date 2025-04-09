using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28676, "Android Dynamic Updates to CollectionView Header and Footer and Templates Are Not Displayed",
		PlatformAffected.Android)]
	public class Issue28676 : ContentPage
	{
		private Label headerLabel;
		private Label footerLabel;
		private CollectionView collectionView;
		private ObservableCollection<string> items;


		public Issue28676()
		{

			items = new ObservableCollection<string>
		{
			"Item 1", "Item 2", "Item 3"
		};

			// Header and Footer Labels
			headerLabel = new Label
			{
				Text = "Initial Header",
				AutomationId = "Issue28676Header",
				BackgroundColor = Colors.LightBlue,
				FontAttributes = FontAttributes.Bold,
				Padding = 10
			};

			footerLabel = new Label
			{
				Text = "Initial Footer",
				AutomationId = "Issue28676Footer",
				BackgroundColor = Colors.LightGray,
				Padding = 10
			};

			// CollectionView
			collectionView = new CollectionView
			{
				ItemsSource = items,
				AutomationId = "Issue28676CollectionView",
				Header = headerLabel,
				Footer = footerLabel,
				ItemTemplate = new DataTemplate(() =>
				{
					var stack = new StackLayout
					{
						Padding = 10,
						Spacing = 10
					};

					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");

					stack.Children.Add(label);
					return stack;
				})
			};

			// Top Grid with 2x2 Buttons
			var topGrid = new Grid
			{
				RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},
				ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star)
			},
				Padding = 10,
				RowSpacing = 10,
				ColumnSpacing = 10
			};

			var button1 = new Button { Text = "Update Header", AutomationId = "Issue28676UpdateHeaderButton", };
			button1.Clicked += (s, e) => collectionView.Header = "UpdatedCollectionViewHeader";

			var button2 = new Button { Text = "Update Footer", AutomationId = "Issue28676UpdateFooterButton", };
			button2.Clicked += (s, e) => collectionView.Footer = "UpdatedCollectionViewFooter";

			var button3 = new Button { Text = "ChangeHeaderView", AutomationId = "Issue28676ChangeHeaderView", };
			button3.Clicked += OnChangeHeaderView;

			var button4 = new Button { Text = "ChangeFooterView", AutomationId = "Issue28676ChangeFooterView", };
			button4.Clicked += OnChangeFooterView;

			topGrid.Add(button1, 0, 0);
			topGrid.Add(button2, 1, 0);
			topGrid.Add(button3, 0, 1);
			topGrid.Add(button4, 1, 1);

			// Main Grid Layout
			var mainGrid = new Grid
			{
				RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
			};

			mainGrid.Add(topGrid, 0, 0);
			mainGrid.Add(collectionView, 0, 1);

			Content = mainGrid;
		}

		private void OnChangeHeaderView(object sender, EventArgs e)
		{
			var newHeaderGrid = new Grid
			{
				Padding = 10,
				AutomationId = "Issue28676HeaderView",
				BackgroundColor = Colors.LightGreen,
				RowDefinitions =
		{
			new RowDefinition { Height = GridLength.Auto }
		},
				ColumnDefinitions =
		{
			new ColumnDefinition { Width = GridLength.Star }
		}
			};

			var headerLabel = new Label
			{
				Text = "Updated HeaderView",
				FontAttributes = FontAttributes.Bold,
				FontSize = 16,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};

			newHeaderGrid.Add(headerLabel, 0, 0);
		}

		private void OnChangeFooterView(object sender, EventArgs e)
		{
			var newHeaderGrid = new Grid
			{
				Padding = 10,
				AutomationId = "Issue28676FooterView",
				BackgroundColor = Colors.LightCyan,
				RowDefinitions =
		{
			new RowDefinition { Height = GridLength.Auto }
		},
				ColumnDefinitions =
		{
			new ColumnDefinition { Width = GridLength.Star }
		}
			};

			var headerLabel = new Label
			{
				Text = "Updated FooterView",
				FontAttributes = FontAttributes.Bold,
				FontSize = 16,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};

			newHeaderGrid.Add(headerLabel, 0, 0);
		}
	}
}

