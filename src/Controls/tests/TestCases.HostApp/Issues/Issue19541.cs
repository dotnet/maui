using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19541, "[iOS] - Swipeview with collectionview issue", PlatformAffected.iOS)]

public class Issue19541 : ContentPage
{
    SwipeView _swipeView;
    CollectionView collectionView;

    public Issue19541()
    {
        // Root layout
        var rootLayout = new VerticalStackLayout();

        // Refresh button
        var refreshButton = new Button
        {
            Text = "Refresh List",
            AutomationId = "RefreshButton",
            Margin = new Thickness(10)
        };
        refreshButton.Clicked += OnRefreshClicked;

        // Open button
        var openButton = new Button
        {
            Text = "Open",
            AutomationId = "OpenButton"
        };
        openButton.Clicked += Button_Clicked;

        // CollectionView
        collectionView = new CollectionView();

        // Define ItemTemplate in code
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var swipeView = new SwipeView
            {
                HeightRequest = 60
            };

            swipeView.Loaded += swipeView_Loaded;

            var label = new Label();
            label.SetBinding(Label.TextProperty, "Name");

            // Swipe RightItems
            var swipeItems = new SwipeItems
            {
                SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
            };

            var swipeItemView = new SwipeItemView
            {
                Content = new Button
                {
                    BackgroundColor = Colors.Pink,
                    Text = "Delete"
                }
            };

            swipeItems.Add(swipeItemView);
            swipeView.RightItems = swipeItems;

            swipeView.Content = label;

            return swipeView;
        });

        // Add controls to layout
        rootLayout.Children.Add(refreshButton);
        rootLayout.Children.Add(openButton);
        rootLayout.Children.Add(collectionView);

        Content = rootLayout;

        LoadInitialList();
    }

    void LoadInitialList()
    {
        var list = new List<Issue19541Model>();
        for (int i = 0; i < 3; i++)
        {
            list.Add(new Issue19541Model
            {
                Name = $"Name {i}",
            });
        }

        collectionView.ItemsSource = list;
    }

    void OnRefreshClicked(object sender, EventArgs e)
    {
        var list = new List<Issue19541Model>();
        for (int i = 0; i < 3; i++)
        {
            list.Add(new Issue19541Model
            {
                Name = $"Name {i}",
            });
        }

        collectionView.ItemsSource = list;
    }

    void swipeView_Loaded(object sender, EventArgs e)
    {
        if (_swipeView is null)
            _swipeView = (SwipeView)sender;
    }

    void Button_Clicked(object sender, EventArgs e)
    {
        _swipeView?.Open(OpenSwipeItem.RightItems);
    }
}

public class Issue19541Model
{
    public string Name { get; set; }
}

