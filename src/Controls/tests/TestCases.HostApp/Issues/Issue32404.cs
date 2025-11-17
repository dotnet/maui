namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32404, "[Android, iOS, MacOS] FlowDirection not working on EmptyView in CollectionView", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32404 : ContentPage
{
    Label flowDirectionLabel;
    CollectionView myCollectionView;
    public Issue32404()
    {
        var grid = new Grid
        {
            Padding = new Thickness(10),
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
        };

        var toggleButton = new Button
        {
            Text = "Toggle FlowDirection",
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10),
            AutomationId = "Issue32404ToggleButton"
        };

        toggleButton.Clicked += OnToggleFlowDirectionClicked;
        grid.Add(toggleButton);
        Grid.SetRow(toggleButton, 0);

        flowDirectionLabel = new Label
        {
            Text = "Current FlowDirection: LeftToRight",
            HorizontalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold
        };
        grid.Add(flowDirectionLabel);
        Grid.SetRow(flowDirectionLabel, 1);

        // CollectionView
        myCollectionView = new CollectionView
        {
            BackgroundColor = Colors.LightGray,
            FlowDirection = FlowDirection.LeftToRight
        };

        // collectionView EmptyView
        myCollectionView.EmptyView = new Label
        {
            Text = "This is empty view"
        };

        grid.Add(myCollectionView);
        Grid.SetRow(myCollectionView, 2);

        // Set Grid as Content
        Content = grid;
    }

    void OnToggleFlowDirectionClicked(object sender, EventArgs e)
    {
        // Toggle between LeftToRight and RightToLeft
        if (myCollectionView.FlowDirection == FlowDirection.LeftToRight)
        {
            myCollectionView.FlowDirection = FlowDirection.RightToLeft;
            flowDirectionLabel.Text = "Current FlowDirection: RightToLeft";
        }
    }
}