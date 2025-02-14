using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26187, "[MAUI] Select items traces are preserved", PlatformAffected.iOS)]
	public class Issue26187 : NavigationPage
	{
		public Issue26187()
		{
			PushAsync(new CollectionViewSelectedItemNullPage());
		}
	}

	public class CollectionViewSelectedItemNullPage : ContentPage
	{
		public ObservableCollection<string> Items { get; set; }

		public string SelectedItem { get; set; }

		public CollectionViewSelectedItemNullPage()
		{
			Items = new ObservableCollection<string>
				{
					"Item 1",
					"Item 2",
					"Item 3",
					"Item 4",
					"Item 5"
				};
			SelectedItem = Items.LastOrDefault();
			var cv = new CollectionView
			{
				SelectionMode = SelectionMode.Single,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						FontSize = 24,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.Center,
						AutomationId = "lblItem"
					};
					label.SetBinding(Label.TextProperty, ".");

					return new HorizontalStackLayout
					{
						Children = { label }
					};
				})

			};

			cv.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(Items)));
			//  cv.SetBinding(CollectionView.SelectedItemProperty, new Binding(nameof(SelectedItem)));
			Content = cv;

			BindingContext = this;
			//  cv.SelectedItem = SelectedItem;

			cv.SelectionChanged += CollectionView_SelectionChanged;
		}

		async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is string issue)
			{
				await Navigation.PushAsync(new NewPage(issue));
			}

			// Clear Selection
			var cv = sender as CollectionView;
			if (cv is not null)
			{
				cv.SelectedItem = null;
			}
		}

		class NewPage : ContentPage
		{
			public NewPage(string item)
			{
				Title = item;
				Content = new Button
				{
					Text = $"Go Back Selected Item null from {item}",
					Command = new Command(() => Navigation.PopAsync()),
					AutomationId = "btnGoBack"
				};
			}
		}
	}
}