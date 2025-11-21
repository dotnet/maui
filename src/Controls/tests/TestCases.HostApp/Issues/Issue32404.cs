namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32404, "[Android, iOS, MacOS] FlowDirection not working on EmptyView in CollectionView", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue32404 : ContentPage
{
    Label flowDirectionLabel;
    CollectionView emptyViewStringCollectionView;
    CollectionView emptyViewViewCollectionView;
    CollectionView emptyViewTemplateCollectionView;

    public Issue32404()
    {
        var grid = new Grid
        {
            Padding = new Thickness(10),
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
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

        // String EmptyView
        emptyViewStringCollectionView = new CollectionView
        {
            BackgroundColor = Colors.LightGray,
            FlowDirection = FlowDirection.LeftToRight,
            EmptyView = "EmptyView Text (String)",
            AutomationId = "CollectionView1"
        };

        // View EmptyView
        emptyViewViewCollectionView = new CollectionView
        {
            BackgroundColor = Colors.LightBlue,
            FlowDirection = FlowDirection.LeftToRight,
            AutomationId = "CollectionView2"
        };

        var emptyViewGrid = new Grid();
        var emptyViewLabel = new Label
        {
            Text = "EmptyView (Grid View)",
        };
        emptyViewGrid.Add(emptyViewLabel);
        emptyViewViewCollectionView.EmptyView = emptyViewGrid;

        // DataTemplate EmptyView
        emptyViewTemplateCollectionView = new CollectionView
        {
            BackgroundColor = Colors.LightGreen,
            FlowDirection = FlowDirection.LeftToRight,
        };

        emptyViewTemplateCollectionView.EmptyViewTemplate = new DataTemplate(() =>
        {
            var stackLayout = new VerticalStackLayout();
            var templateLabel = new Label
            {
                Text = "EmptyView Template",
            };

            stackLayout.Add(templateLabel);
            return stackLayout;
        });

        grid.Add(emptyViewStringCollectionView);
        Grid.SetRow(emptyViewStringCollectionView, 2);
        grid.Add(emptyViewViewCollectionView);
        Grid.SetRow(emptyViewViewCollectionView, 3);
        grid.Add(emptyViewTemplateCollectionView);
        Grid.SetRow(emptyViewTemplateCollectionView, 4);
        // Set Grid as Content
        Content = grid;
    }

    void OnToggleFlowDirectionClicked(object sender, EventArgs e)
    {
        // Toggle between LeftToRight and RightToLeft
        if (emptyViewStringCollectionView.FlowDirection == FlowDirection.LeftToRight)
        {
            emptyViewStringCollectionView.FlowDirection = FlowDirection.RightToLeft;
            emptyViewViewCollectionView.FlowDirection = FlowDirection.RightToLeft;
            emptyViewTemplateCollectionView.FlowDirection = FlowDirection.RightToLeft;
            flowDirectionLabel.Text = "Current FlowDirection: RightToLeft";
        }
        else
        {
            emptyViewStringCollectionView.FlowDirection = FlowDirection.LeftToRight;
            emptyViewViewCollectionView.FlowDirection = FlowDirection.LeftToRight;
            emptyViewTemplateCollectionView.FlowDirection = FlowDirection.LeftToRight;
            flowDirectionLabel.Text = "Current FlowDirection: LeftToRight";
        }
    }
}