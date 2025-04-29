namespace Maui.Controls.Sample.Issues;

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 29170, "First Item in CollectionView Overlaps in FlyoutPage.Flyout on iOS", PlatformAffected.iOS)]
public partial class Issue29170 : Microsoft.Maui.Controls.FlyoutPage
{
    public Issue29170()
    {
        var flyoutPage = new ContentPage
        {
            Title = "Menu",
            Content = new CollectionView
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3", "Item 4" },
                AutomationId = "CollectionView",
                ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
                Margin = 10,
                ItemTemplate = new DataTemplate(() =>
                {
                    var titleLabel = new Label
                    {
                        FontSize = 32,
                        LineBreakMode = LineBreakMode.TailTruncation
                    };
                    titleLabel.SetBinding(Label.TextProperty, new Binding("."));

                    var subHeaderLabel = new Label
                    {
                        FontSize = 16,
                        Opacity = 0.66,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        Text = "subheader"
                    };

                    return new VerticalStackLayout
                    {
                        Padding = new Thickness(5),
                        Children = { titleLabel, subHeaderLabel }
                    };
                })
            }
        };
        
        Flyout = flyoutPage;

        var toggleButton = new Button
        {
            Text = "Open Flyout Menu",
            FontSize = 24,
            AutomationId = "FlyoutButton",
            Margin = 10,
            WidthRequest = 220,
            HeightRequest = 50,
            VerticalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.Blue
        };
        toggleButton.Clicked += ToggleFlyoutMenu;

        Detail = new ContentPage
        {
            Content = toggleButton
        };
        
        this.On<iOS>().SetUseSafeArea(true);
    }

    private void ToggleFlyoutMenu(object sender, EventArgs e) => IsPresented = !IsPresented;
}
