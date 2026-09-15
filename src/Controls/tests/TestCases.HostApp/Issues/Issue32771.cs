namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32771, "Flow direction not working on Header/Footer in CollectionView [iOS]", PlatformAffected.iOS | PlatformAffected.macOS)]

public class Issue32771 : ContentPage
{
    public Issue32771()
    {
        Grid grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        CollectionView2 headerFooterView = new CollectionView2
        {
            EmptyView = "No items to display (String)",
            Header = new Grid
            {
                Children =
                {
                    new Label
                    {
                    Text = "Header View",
                    }
                }
            },
            Footer = new Grid
            {
                Children =
                {
                    new Label
                    {
                    Text = "Footer View"
                    }
                }
            }
        };
        headerFooterView.FlowDirection = FlowDirection.LeftToRight;
        Grid.SetRow(headerFooterView, 2);
        CollectionView2 headerFooterStringView = new CollectionView2
        {
            EmptyView = "No items to display (String)",
            Header = "Header String",
            Footer = "Footer String",
            Background = Colors.Gray
        };
        headerFooterStringView.FlowDirection = FlowDirection.LeftToRight;
        Grid.SetRow(headerFooterStringView, 1);

        CollectionView2 headerFooterTemplateView = new CollectionView2
        {
            EmptyView = "No items to display (String)",
            HeaderTemplate = new DataTemplate(() =>
            {
                return new Label
                {
                    Text = "Header Template",
                };
            }),
            FooterTemplate = new DataTemplate(() =>
            {
                return new Label
                {
                    Text = "Footer Template",
                };
            }),
            Background = Colors.LightBlue
        };
        headerFooterTemplateView.FlowDirection = FlowDirection.LeftToRight;
        Grid.SetRow(headerFooterTemplateView, 3);

        Label label = new Label
        {
            Text = "Current FlowDirection: LeftToRight",
            HorizontalOptions = LayoutOptions.Center
        };

        Button button = new Button
        {
            Text = "Toggle FlowDirection",
            AutomationId = "ToggleFlowDirectionButton"
        };
        button.Clicked += (s, e) =>
        {
            if (headerFooterView.FlowDirection == FlowDirection.LeftToRight)
            {
                headerFooterView.FlowDirection = FlowDirection.RightToLeft;
                headerFooterStringView.FlowDirection = FlowDirection.RightToLeft;
                headerFooterTemplateView.FlowDirection = FlowDirection.RightToLeft;
                label.Text = "Current FlowDirection: RightToLeft";
            }
            else
            {
                headerFooterView.FlowDirection = FlowDirection.LeftToRight;
                headerFooterStringView.FlowDirection = FlowDirection.LeftToRight;
                headerFooterTemplateView.FlowDirection = FlowDirection.LeftToRight;
                label.Text = "Current FlowDirection: LeftToRight";
            }
        };
        Grid.SetRow(button, 0);
        grid.Children.Add(button);
        Grid.SetRow(label, 1);
        grid.Children.Add(label);
        Grid.SetRow(headerFooterStringView, 2);
        grid.Children.Add(headerFooterStringView);
        Grid.SetRow(headerFooterView, 3);
        grid.Children.Add(headerFooterView);
        Grid.SetRow(headerFooterTemplateView, 4);
        grid.Children.Add(headerFooterTemplateView);
        Content = grid;
    }
}