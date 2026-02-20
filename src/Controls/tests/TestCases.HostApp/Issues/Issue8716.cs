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

    public class _8716CatsPage : ContentPage
    {
        _8716AnimalSearchHandler searchHandler;
        public _8716CatsPage()
        {
            Title = "CatsPage";

            searchHandler = new _8716AnimalSearchHandler
            {
                Placeholder = "Enter cat's name",
                CancelButtonColor = Colors.Green,
                ShowsResults = true,
                Animals = new List<string>() { "Abyssinian", "Arabian Mau" },
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
                    nameLabel.SetBinding(Label.TextProperty, ".");
					grid.Children.Add(nameLabel);

					return grid;
                })
            };

            Shell.SetSearchHandler(this, searchHandler);

            var collectionView = new CollectionView
            {
                AutomationId = "MainPageCollectionView",
                Margin = new Thickness(20),
                ItemsSource = new List<string>() { "Abyssinian", "Arabian Mau" },
                ItemTemplate = new DataTemplate(() =>
                {
                    var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                    nameLabel.SetBinding(Label.TextProperty, ".");
                    return nameLabel;
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
                Placeholder = "Enter dog's name",
                ShowsResults = true,
                AutomationId = "AnimalSearchHandler",
                Animals = new List<string>() { "Afghan Hound", "Alpine Dachsbracke" },
                ItemTemplate = new DataTemplate(() =>
                {
                    var nameLabel = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center
                    };
                    nameLabel.SetBinding(Label.TextProperty, ".");
                    return nameLabel;
                })
            };

            Shell.SetSearchHandler(this, searchHandler);

            var collectionView = new CollectionView
            {
                Margin = new Thickness(20),
                ItemsSource = new List<string>() { "Afghan Hound", "Alpine Dachsbracke" },
                ItemTemplate = new DataTemplate(() =>
                {
                    var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
                    nameLabel.SetBinding(Label.TextProperty, ".");
                    return nameLabel;
                }),
                SelectionMode = SelectionMode.Single
            };

            var button = new Button
            {
                Text = "DogPageButton",
                AutomationId = "DogPageButton"
            };

            var layout = new StackLayout
            {
                Children = { button, collectionView }
            };

            Content = layout;
        }
    }

    public class _8716AnimalSearchHandler : SearchHandler
    {
        public IList<string> Animals { get; set; }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);
            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                var filteredAnimals = new List<string>();
                if (Animals != null)
                {
                    foreach (var animal in Animals)
                    {
                        if (animal.IndexOf(newValue, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            filteredAnimals.Add(animal);
                        }
                    }
                }

                ItemsSource = filteredAnimals;
            }
        }
    }
}