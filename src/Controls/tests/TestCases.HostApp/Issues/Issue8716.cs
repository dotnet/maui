namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8716, "[Shell][Android] The truth is out there...but not on top tab search handlers", PlatformAffected.Android)]
public class Issue8716 : Shell
{
	public Issue8716()
	{
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
			Title = "CatsPage",
			Route = "cats",
			ContentTemplate = new DataTemplate(typeof(_8716CatsPage))
		});

		domesticTab.Items.Add(new ShellContent
		{
			Title = "DogsPage",
			Route = "dogs",
			ContentTemplate = new DataTemplate(typeof(_8716DogsPage))
		});

		flyoutItem.Items.Add(domesticTab);

		Items.Add(flyoutItem);
	}

	public class _8716Animal
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public string Details { get; set; }
	}

	public class _8716CatsPage : ContentPage
	{
		_8716AnimalSearchHandler searchHandler;
		public _8716CatsPage()
		{
			Title = "CatsPage";

			searchHandler = new _8716AnimalSearchHandler
			{
				Placeholder = "Enter Cat name",
				CancelButtonColor = Colors.Green,
				ShowsResults = true,
				Animals= _8716CatData.Cats,
				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						Padding = 10
					};

					var nameLabel = new Label
					{
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.Center
					};
					nameLabel.SetBinding(Label.TextProperty, "Name");

					grid.Children.Add(nameLabel);

					return grid;
				})
			};

			Shell.SetSearchHandler(this, searchHandler);

			var collectionView = new CollectionView
			{
				AutomationId = "MainPageCollectionView",
				Margin = new Thickness(20),
				ItemsSource = _8716CatData.Cats,
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

			var button = new Button
			{
				Text = "Button",
				AutomationId = "MainPageButton"
			};

			var layout = new StackLayout
			{
				Children = { button, collectionView }
			};

			Content = layout;
		}
	}

	public class _8716DogsPage : ContentPage
	{
		_8716AnimalSearchHandler searchHandler;
		public _8716DogsPage()
		{
			Title = "DogsPage";

			searchHandler = new _8716AnimalSearchHandler
			{
				Placeholder = "Enter Dog name",
				ShowsResults = true,
				AutomationId = "AnimalSearchHandler",
				Animals = _8716DogData.Dogs,
				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						Padding = 10
					};

					var nameLabel = new Label
					{
						FontAttributes = FontAttributes.Bold,
						VerticalOptions = LayoutOptions.Center
					};
					nameLabel.SetBinding(Label.TextProperty, "Name");

					grid.Children.Add(nameLabel);

					return grid;
				})
			};

			Shell.SetSearchHandler(this, searchHandler);

			var collectionView = new CollectionView
			{
				Margin = new Thickness(20),
				ItemsSource = _8716DogData.Dogs,
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

			var layout = new StackLayout
			{
				Children = { collectionView }
			};

			Content = layout;
		}
	}

	public static class _8716DogData
	{
		public static IList<_8716Animal> Dogs { get; set; }

		static _8716DogData()
		{
			Dogs = new List<_8716Animal>();

			Dogs.Add(new _8716Animal
            {
                Name = "Afghan Hound",
                Location = "Afghanistan",
                Details = "The Afghan Hound is a hound that is distinguished by its thick, fine, silky coat and its tail with a ring curl at the end. The breed is selectively bred for its unique features in the cold mountains of Afghanistan.  Other names for this breed are Kuchi Hound, Tāzī, Balkh Hound, Baluchi Hound, Barakzai Hound, Shalgar Hound, Kabul Hound, Galanday Hound or sometimes incorrectly African Hound.",
            });

            Dogs.Add(new _8716Animal
            {
                Name = "Alpine Dachsbracke",
                Location = "Austria",
                Details = "The Alpine Dachsbracke is a small breed of dog of the scent hound type originating in Austria. The Alpine Dachsbracke was bred to track wounded deer as well as boar, hare, and fox. It is highly efficient at following a trail even after it has gone cold. The Alpine Dachsbracke is very sturdy, and Austria is said to be the country of origin.",
            });
		}
	}

	public static class _8716CatData
	{
		public static IList<_8716Animal> Cats { get; set; }

		static _8716CatData()
		{
			Cats = new List<_8716Animal>();

			Cats.Add(new _8716Animal
			{
				Name = "Abyssinian",
				Location = "Ethiopia",
				Details = "The Abyssinian is a breed of domestic short-haired cat."
			});

			Cats.Add(new _8716Animal
			{
				Name = "Arabian Mau",
				Location = "Arabian Peninsula",
				Details = "The Arabian Mau is a formal breed of domestic cat."
			});
		}
	}

	public class _8716AnimalSearchHandler : SearchHandler
	{
		public IList<_8716Animal> Animals { get; set; }

		protected override void OnQueryChanged(string oldValue, string newValue)
		{
			base.OnQueryChanged(oldValue, newValue);

			if (string.IsNullOrWhiteSpace(newValue))
			{
				ItemsSource = null;
			}
			else
			{
				var filteredAnimals = new List<_8716Animal>();
				if (Animals != null)
				{
					foreach (var animal in Animals)
					{
						if (animal.Name != null && animal.Name.IndexOf(newValue, StringComparison.CurrentCultureIgnoreCase) >= 0)
						{
							filteredAnimals.Add(animal);
						}
					}
				}
				ItemsSource = filteredAnimals;
			}
		}
	}

	[QueryProperty(nameof(Name), "name")]
	public class _8716CatDetailsPage : ContentPage
	{
		public string Name
		{
			set
			{
				LoadAnimal(value);
			}
		}

		public _8716CatDetailsPage()
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
			foreach (var animal in _8716CatData.Cats)
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