namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33356, "Navigate should occur when an item got selected", PlatformAffected.iOS)]
public class Issue33356 : Shell
{
	public Issue33356()
	{
		Routing.RegisterRoute("Issue33356CatDetails", typeof(_33356CatDetailsPage));

		var flyoutItem = new FlyoutItem
		{
			Route = "animals",
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
		};

		var domesticTab = new Tab
		{
			Title = "Domestic",
			Route = "domestic"
		};

		domesticTab.Items.Add(new ShellContent
		{
			Title = "Cats",
			Route = "cats",
			ContentTemplate = new DataTemplate(typeof(_33356CatsPage))
		});

		flyoutItem.Items.Add(domesticTab);

		Items.Add(flyoutItem);
	}

	public class _33356Animal
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
	}

	public class _33356CatsPage : ContentPage
	{
		public _33356CatsPage()
		{
			Title = "Cats";

			var searchHandler = new _33356AnimalSearchHandler
			{
				Placeholder = "Enter search term (try 'A')",
				ShowsResults = true,
				Animals = _33356CatData.Cats,
				ItemsSource = _33356CatData.Cats,
				AutomationId = "Issue33356SearchHandler",
				ItemTemplate = new DataTemplate(() =>
				{
					var nameLabel = new Label
					{
						AutomationId = "SearchResultName",
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.Center
					};
					nameLabel.SetBinding(Label.TextProperty, "Name");

					return nameLabel;
				})
			};

			Shell.SetSearchHandler(this, searchHandler);



			var collectionView = new CollectionView
			{
				AutomationId = "Issue33356CatsCollectionView",
				Margin = new Thickness(20),
				ItemsSource = _33356CatData.Cats,
				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						Padding = 10,
						RowDefinitions = new RowDefinitionCollection
						{
							new RowDefinition { Height = GridLength.Auto },
							new RowDefinition { Height = GridLength.Auto }
						}
					};

					var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
					nameLabel.SetBinding(Label.TextProperty, "Name");

					var locationLabel = new Label
					{
						FontAttributes = FontAttributes.Italic,
						VerticalOptions = LayoutOptions.End
					};
					locationLabel.SetBinding(Label.TextProperty, "Location");

					grid.Children.Add(nameLabel);
					Grid.SetRow(nameLabel, 0);

					grid.Children.Add(locationLabel);
					Grid.SetRow(locationLabel, 1);

					return grid;
				}),
				SelectionMode = SelectionMode.Single
			};

			collectionView.SelectionChanged += (s, e) =>
			{
				if (e.CurrentSelection.Count == 0)
					return;

				var animal = e.CurrentSelection.FirstOrDefault() as _33356Animal;
				if (animal?.Name == null)
					return;

				string catName = animal.Name;
				// The following route works because route names are unique in this application.
				Shell.Current.GoToAsync($"Issue33356CatDetails?name={catName}");
			};

			var layout = new StackLayout
			{
				Children = { collectionView }
			};

			Content = layout;
		}
	}

	public static class _33356CatData
	{
		public static IList<_33356Animal> Cats { get; set; }

		static _33356CatData()
		{
			Cats = new List<_33356Animal>();

			Cats.Add(new _33356Animal
			{
				Name = "Abyssinian",
				Location = "Ethiopia",
				Details = "The Abyssinian is a breed of domestic short-haired cat."
			});

			Cats.Add(new _33356Animal
			{
				Name = "Arabian Mau",
				Location = "Arabian Peninsula",
				Details = "The Arabian Mau is a formal breed of domestic cat."
			});
		}
	}

	public class _33356AnimalSearchHandler : SearchHandler
	{
		public IList<_33356Animal> Animals { get; set; }

		public _33356AnimalSearchHandler()
		{
			Animals = new List<_33356Animal>();
		}
		protected override async void OnItemSelected(object item)
		{
			base.OnItemSelected(item);

			try
			{
				if (item is not _33356Animal animal)
				{
					return;
				}
				// Navigate, passing a string
				var route = $"Issue33356CatDetails?name={animal.Name}";
				await Shell.Current.GoToAsync(route);

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in OnItemSelected: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
			}
		}
	}

	[QueryProperty(nameof(Name), "name")]
	public class _33356CatDetailsPage : ContentPage
	{
		public string Name
		{
			set
			{
				LoadAnimal(value);
			}
		}

		public _33356CatDetailsPage()
		{
			Title = "Cat Details";

			var scrollView = new ScrollView();
			var stackLayout = new StackLayout { Margin = new Thickness(20) };

			var nameLabel = new Label
			{
				AutomationId = "Issue33356CatNameLabel",
				HorizontalOptions = LayoutOptions.Center,
				FontSize = 24,
				FontAttributes = FontAttributes.Bold
			};
			nameLabel.SetBinding(Label.TextProperty, "Name");

			var locationLabel = new Label
			{
				AutomationId = "CatLocationLabel",
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.Center
			};
			locationLabel.SetBinding(Label.TextProperty, "Location");

			var detailsLabel = new Label
			{
				AutomationId = "CatDetailsLabel",
				HorizontalOptions = LayoutOptions.Center,
				LineBreakMode = LineBreakMode.WordWrap,
				VerticalOptions = LayoutOptions.Start
			};
			detailsLabel.SetBinding(Label.TextProperty, "Details");

			stackLayout.Children.Add(nameLabel);
			stackLayout.Children.Add(locationLabel);
			stackLayout.Children.Add(detailsLabel);

			scrollView.Content = stackLayout;
			Content = scrollView;
		}

		void LoadAnimal(string name)
		{
			foreach (var animal in _33356CatData.Cats)
			{
				if (animal.Name == name)
				{
					BindingContext = animal;
					return;
				}
			}

			// If animal not found, show a default message
			BindingContext = new _33356Animal
			{
				Name = "Unknown Cat",
				Location = "Unknown",
				Details = "Cat details not found."
			};
		}
	}
}

