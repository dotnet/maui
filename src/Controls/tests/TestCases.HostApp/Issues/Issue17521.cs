namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17521, "Shell.SearchHandler visible in details page on Windows 11", PlatformAffected.UWP)]
public class Issue17521 : Shell
{
	public Issue17521()
	{
		Routing.RegisterRoute("CatDetails", typeof(_17521CatDetailsPage));
		this.FlyoutBehavior = FlyoutBehavior.Flyout;
		this.FlyoutBackgroundImageAspect = Aspect.AspectFill;
		this.FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;

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
			ContentTemplate = new DataTemplate(typeof(_17521CatsPage))
		});

		domesticTab.Items.Add(new ShellContent
		{
			Title = "Dogs",
			Route = "dogs",
			ContentTemplate = new DataTemplate(typeof(_17521DogsPage))
		});

		flyoutItem.Items.Add(domesticTab);

		Items.Add(flyoutItem);
	}

	public class _17521Animal
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
	}

	public class _17521CatsPage : ContentPage
	{
		public _17521CatsPage()
		{
			Title = "Cats";

			var searchHandler = new _17521AnimalSearchHandler
			{
				Placeholder = "Enter search term",
				ShowsResults = true,
				ItemsSource = _17521CatData.Cats,
				ItemTemplate = new DataTemplate(() =>
				{
					var nameLabel = new Label
					{
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
				Margin = new Thickness(20),
				ItemsSource = _17521CatData.Cats,
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

			collectionView.SelectionChanged += async (s, e) =>
			{
				if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
				{
					foreach (var item in e.CurrentSelection)
					{
						var selectedAnimal = item as _17521Animal;
						if (selectedAnimal != null)
						{
							await Shell.Current.GoToAsync($"CatDetails?name={selectedAnimal.Name}");
							break;
						}
					}
					collectionView.SelectedItem = null;
				}
			};

			var button = new Button
			{
				Text = "Select First Cat",
				AutomationId = "MainButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.End
			};

			button.Clicked += (s, e) =>
			{
				collectionView.SelectedItem = _17521CatData.Cats[0];
			};

			var layout = new StackLayout
			{
				Children = { button, collectionView }
			};

			Content = layout;
		}
	}

	public class _17521DogsPage : ContentPage
	{
		public _17521DogsPage()
		{
			Title = "Dogs";
			Content = new Label
			{
				Text = "Welcome to the Dogs Page!",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}

	public static class _17521CatData
	{
		public static IList<_17521Animal> Cats { get; set; }

		static _17521CatData()
		{
			Cats = new List<_17521Animal>();

			Cats.Add(new _17521Animal
			{
				Name = "Abyssinian",
				Location = "Ethiopia",
				Details = "The Abyssinian is a breed of domestic short-haired cat."
			});

			Cats.Add(new _17521Animal
			{
				Name = "Arabian Mau",
				Location = "Arabian Peninsula",
				Details = "The Arabian Mau is a formal breed of domestic cat."
			});
		}
	}

	public class _17521AnimalSearchHandler : SearchHandler
	{
		public IList<_17521Animal> Animals { get; set; }

		protected override void OnQueryChanged(string oldValue, string newValue)
		{
			base.OnQueryChanged(oldValue, newValue);

			if (string.IsNullOrWhiteSpace(newValue))
			{
				ItemsSource = null;
			}
			else
			{
				var filteredAnimals = new List<_17521Animal>();
				foreach (var animal in Animals)
				{
					if (animal.Name.IndexOf(newValue, StringComparison.CurrentCultureIgnoreCase) >= 0)
					{
						filteredAnimals.Add(animal);
					}
				}
				ItemsSource = filteredAnimals;
			}
		}
	}

	[QueryProperty(nameof(Name), "name")]
	public partial class _17521CatDetailsPage : ContentPage
	{
		public string Name
		{
			set
			{
				LoadAnimal(value);
			}
		}

		public _17521CatDetailsPage()
		{
			var scrollView = new ScrollView();
			var stackLayout = new StackLayout { Margin = new Thickness(20) };

			var nameLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
			};
			nameLabel.SetBinding(Label.TextProperty, "Name");

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				HorizontalOptions = LayoutOptions.Center
			};
			locationLabel.SetBinding(Label.TextProperty, "Location");

			var detailsLabel = new Label
			{
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
			foreach (var animal in _17521CatData.Cats)
			{
				if (animal.Name == name)
				{
					BindingContext = animal;
					return;
				}
			}

			throw new Exception("Animal not found.");
		}
	}
}

