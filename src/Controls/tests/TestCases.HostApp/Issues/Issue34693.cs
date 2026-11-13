namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34693, "[Shell][iOS & Mac] SearchHandler retains previous page state when switching top tabs", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34693 : Shell
{
    public Issue34693()
    {
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
            ContentTemplate = new DataTemplate(typeof(_34693CatsPage))
        });

        domesticTab.Items.Add(new ShellContent
        {
            Title = "DogsPage",
            Route = "dogs",
            ContentTemplate = new DataTemplate(typeof(_34693DogsPage))
        });

        flyoutItem.Items.Add(domesticTab);

        Items.Add(flyoutItem);
    }

    public class _34693CatsPage : ContentPage
    {
        _34693AnimalSearchHandler searchHandler;
        public _34693CatsPage()
        {
            Title = "CatsPage";

            searchHandler = new _34693AnimalSearchHandler
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

    public class _34693DogsPage : ContentPage
    {
        _34693AnimalSearchHandler searchHandler;
        public _34693DogsPage()
        {
            Title = "DogsPage";

            searchHandler = new _34693AnimalSearchHandler
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

    public class _34693AnimalSearchHandler : SearchHandler
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
