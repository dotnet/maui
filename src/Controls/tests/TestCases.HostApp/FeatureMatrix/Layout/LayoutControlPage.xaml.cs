using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class LayoutControlPage : NavigationPage
{
    private LayoutViewModel _viewModel;
    public LayoutControlPage()
    {
        _viewModel = new LayoutViewModel();

        PushAsync(new LayoutMainPage(_viewModel));
    }
}

public partial class LayoutMainPage : ContentPage
{
    private LayoutViewModel _viewModel;

    public LayoutMainPage(LayoutViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        InitializeContent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeContent(); // reinitialize every time page appears
    }

    private void InitializeContent()
    {
        var defaultLayout = new VerticalStackLayout
        {
            BackgroundColor = Colors.LightGray,
            Children =
            {
                new Label { Text = "Welcome to LayoutPage" },
            }
        };

        MyScrollView.Content = defaultLayout;
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new LayoutViewModel();
        await Navigation.PushAsync(new LayoutOptionsPage(_viewModel));
    }

    private void OnScrollViewWithStackLayoutClicked(object sender, EventArgs e)
    {
        var layout = new VerticalStackLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.LightGray,
            Children =
        {
            new Label { Text = "ScrollView contains direct Layout" },
            new Button { Text = "Click Me" }
        }
        };

        MyScrollView.Content = layout;
    }
    private void OnContentViewWithStackLayoutClicked(object sender, EventArgs e)
    {
        var innerLayout = new VerticalStackLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.LightBlue,
            Padding = 10,
            Spacing = 8,
            Children =
        {
            new Label { Text = "Layout inside ContentView", FontSize = 18, FontAttributes = FontAttributes.Bold }
        }
        };

        for (int i = 1; i <= 5; i++)
        {
            innerLayout.Children.Add(new Button { Text = $"Add Item {i}" });
        }

        MyScrollView.Content = new ContentView { Content = innerLayout };
    }

    private void OnGridWithStackLayoutClicked(object sender, EventArgs e)
    {
        var stack = new VerticalStackLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.Beige,
            Padding = 10,
            Spacing = 8,
            Children =
        {
            new Label { Text = "Layout inside Grid", FontSize = 18, FontAttributes = FontAttributes.Bold }
        }
        };

        for (int i = 1; i <= 5; i++)
        {
            stack.Children.Add(new Button { Text = $"Vegetable {i}" });
        }

        var grid = new Grid();
        grid.Add(stack);

        MyScrollView.Content = grid;
    }
    private void OnNestedStackLayoutsClicked(object sender, EventArgs e)
    {
        var innerStack = new VerticalStackLayout
        {
            Padding = 10,
            Spacing = 6,
            BackgroundColor = Colors.LightPink
        };

        innerStack.Children.Add(new Label
        {
            Text = "Inner Stack",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        });

        for (int i = 1; i <= 2; i++)
        {
            innerStack.Children.Add(new Button { Text = $"Fruit {i}" });
        }

        for (int i = 1; i <= 2; i++)
        {
            innerStack.Children.Add(new Button { Text = $"Vegetable {i}" });
        }

        var parentLayout = new VerticalStackLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.LightGray,
            Padding = 10,
            Spacing = 10,
            Children =
        {
            innerStack
        }
        };

        var contentView = new ContentView
        {
            Content = parentLayout,
            Padding = 10,

        };

        MyScrollView.Content = contentView;
    }


    private void OnHorizontalStackLayoutClicked(object sender, EventArgs e)
    {
        var horizontalLayout = new HorizontalStackLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.MistyRose,
            Spacing = 10,
            Padding = 10,
            Children =
        {
            new Label { Text = "Layout", FontSize = 16, FontAttributes = FontAttributes.Bold }
        }
        };

        for (int i = 1; i <= 2; i++)
        {
            horizontalLayout.Children.Add(new Button { Text = $"Item {i}" });
        }

        MyScrollView.Content = horizontalLayout;
    }

    private void OnGridWithChildrenClicked(object sender, EventArgs e)
    {
        var grid = new Grid
        {
            Padding = 10,
            BackgroundColor = Colors.LightGray,
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions
        };


        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });


        int buttonCount = 3;
        for (int i = 0; i < buttonCount; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Add(new Button
            {
                Text = $"Button {i + 1}"
            }, 0, i);
        }

        MyScrollView.Content = grid;
    }

    private void OnGridWithContentViewClicked(object sender, EventArgs e)
    {
        int columns = 3;
        int rows = 3;

        var grid = new Grid()
        {
            BackgroundColor = Colors.LightGray,
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
        };

        for (int row = 3; row < rows; row++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int col = 0; col < columns; col++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col + 1;
                var cell = new Label
                {
                    Text = $"Item {index}",
                    FontSize = 16,
                    Padding = new Thickness(8),
                    HorizontalOptions = LayoutOptions.Center,

                };
                grid.Add(cell, col, row);
            }
        }

        var contentView = new ContentView
        {
            Content = grid,
        };

        MyScrollView.Content = contentView;
    }

    private void OnScrollViewWithAbsoluteLayoutClicked(object sender, EventArgs e)
    {
        var absoluteLayout = new AbsoluteLayout
        {
            HorizontalOptions = _viewModel.HorizontalOptions,
            VerticalOptions = _viewModel.VerticalOptions,
            BackgroundColor = Colors.LightGray,
        };

        var box = new BoxView
        {
            Color = Colors.Teal,
            WidthRequest = 120,
            HeightRequest = 120
        };


        absoluteLayout.SizeChanged += (s, args) =>
        {
            double x = (absoluteLayout.Width - box.WidthRequest) / 2;
            double y = (absoluteLayout.Height - box.HeightRequest) / 2;

            AbsoluteLayout.SetLayoutBounds(box, new Rect(x, y, box.WidthRequest, box.HeightRequest));
        };

        absoluteLayout.Children.Add(box);

        MyScrollView.Content = absoluteLayout;
    }

}