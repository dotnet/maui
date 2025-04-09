using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Android Dynamic Updates to CollectionView Header and FooterTemplates Are Not Displayed",
		PlatformAffected.Android)]
	public class Issue28676Template : ContentPage
	{
		private CollectionView collectionView;
		private ObservableCollection<string> items;


		public Issue28676Template()
		{

			items = new ObservableCollection<string>
		{
			"Item 1", "Item 2", "Item 3"
		};

			// CollectionView
			collectionView = new CollectionView
			{
				ItemsSource = items,
				AutomationId = "Issue28676CollectionView",

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
			collectionView.HeaderTemplate = new DataTemplate(() =>
			{
				var stack = new StackLayout
				{
					Padding = 10,
					Spacing = 10
				};

				var label = new Label();
				label.Text = "Initial Header";
				label.AutomationId = "Issue28676TemplateHeader";

				stack.Children.Add(label);
				return stack;
			});
			collectionView.FooterTemplate = new DataTemplate(() =>
			{
				var stack = new StackLayout
				{
					Padding = 10,
					Spacing = 10
				};

				var label = new Label();
				label.Text = "Initial Header";
				label.AutomationId = "Issue28676TemplateFooter";

				stack.Children.Add(label);
				return stack;
			});
			// Top Grid with 2x2 Buttons
			var topGrid = new Grid
			{
				RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},

				Padding = 10,
				RowSpacing = 10
			};

			var button3 = new Button { Text = "ChangeHeaderTemplate", AutomationId = "Issue28676ChangeHeaderTemplate", };
			button3.Clicked += OnChangeHeaderTemplate;

			var button4 = new Button { Text = "ChangeFooterTemplate", AutomationId = "Issue28676ChangeFooterTemplate", };
			button4.Clicked += OnChangeFooterTemplate;

			topGrid.Add(button3, 0);
			topGrid.Add(button4, 1);

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

		private void OnChangeHeaderTemplate(object sender, EventArgs e)
		{
			collectionView.HeaderTemplate = new DataTemplate(() =>
			{
				var stack = new StackLayout
				{
					Padding = 10,
					Spacing = 10
				};

				var label = new Label();
				label.Text = "Changed HeaderTemplate";
				label.AutomationId = "Issue28676ChangedHeaderTemplate";

				stack.Children.Add(label);
				return stack;
			});
		}

		private void OnChangeFooterTemplate(object sender, EventArgs e)
		{
			collectionView.FooterTemplate = new DataTemplate(() =>
			{
				var stack = new StackLayout
				{
					Padding = 10,
					Spacing = 10
				};

				var label = new Label();
				label.Text = "Changed FooterTemplate";
				label.AutomationId = "Issue28676ChangedFooterTemplate";

				stack.Children.Add(label);
				return stack;
			});
		}
	}
}
