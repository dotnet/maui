using System.Linq;
using System;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public partial class ShellFlyoutOptionsPage : ContentPage
{
    private readonly ShellViewModel _viewModel;

    public ShellFlyoutOptionsPage(ShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnApplyClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private void OnFlyoutTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not RadioButton rb || !e.Value)
            return;

        _viewModel.SelectedFlyoutTemplate = rb.Content?.ToString();
        switch (_viewModel.SelectedFlyoutTemplate)
        {
            case "None":
                _viewModel.FlyoutContent = null;
                _viewModel.FlyoutContentTemplate = null;
                break;

            case "FlyoutContent":
                _viewModel.FlyoutContent = new StackLayout
                {
                    Padding = 10,
                    Children =
                    {
                        new Label
                        {
                            Text = "FlyoutContent",
                            AutomationId = "FlyoutContent",
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Button
                        {
                            Text = "Close Flyout",
                            AutomationId = "CloseFlyoutContentButton",
                            Command = new Command(() =>
                            {
                                if (Shell.Current is { } currentShell)
                                    currentShell.FlyoutIsPresented = false;
                            })
                        }
                    }
                };
                _viewModel.FlyoutContentTemplate = null;
                break;

            case "FlyoutContentTemplate":
                _viewModel.FlyoutContentTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Text = "FlyoutContentTemplate",
                        AutomationId = "FlyoutContentTemplate",
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold
                    };

                    var button = new Button
                    {
                        Text = "Close Flyout",
                        AutomationId = "CloseFlyoutContentTemplateButton"
                    };
                    button.Command = new Command(() =>
                    {
                        if (Shell.Current is { } currentShell)
                            currentShell.FlyoutIsPresented = false;
                    });

                    return new StackLayout
                    {
                        Padding = 10,
                        Children = { label, button }
                    };
                });
                _viewModel.FlyoutContent = null;
                break;

            default:
                // Reset to default when no template is selected
                _viewModel.FlyoutContent = null;
                _viewModel.FlyoutContentTemplate = null;
                break;
        }
    }

    private void OnFlyoutBehaviorChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutBehavior = rb.Content?.ToString() switch
            {
                "Disabled" => FlyoutBehavior.Disabled,
                "Flyout" => FlyoutBehavior.Flyout,
                "Locked" => FlyoutBehavior.Locked,
                _ => FlyoutBehavior.Flyout
            };
        }
    }
    private void OnFlyoutHeaderBehaviorChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutHeaderBehavior = rb.Content?.ToString() switch
            {
                "Default" => FlyoutHeaderBehavior.Default,
                "Fixed" => FlyoutHeaderBehavior.Fixed,
                "Scroll" => FlyoutHeaderBehavior.Scroll,
                "CollapseOnScroll" => FlyoutHeaderBehavior.CollapseOnScroll,
                _ => FlyoutHeaderBehavior.Default
            };
        }
    }
    private void OnFlyoutWidthChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel == null)
            return;
        if (double.TryParse(e.NewTextValue, out double width))
        {
            _viewModel.FlyoutWidth = width;
        }
    }
    private void OnFlyoutHeightChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel == null)
            return;
        if (double.TryParse(e.NewTextValue, out double height))
        {
            _viewModel.FlyoutHeight = height;
        }
    }
    private void OnFlyoutBackgroundImageAspectChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutBackgroundImageAspect = rb.Content?.ToString() switch
            {
                "AspectFill" => Aspect.AspectFill,
                "AspectFit" => Aspect.AspectFit,
                "Fill" => Aspect.Fill,
                _ => Aspect.AspectFill
            };
        }
    }

    private void OnBackgroundImageChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutBackgroundImage = rb.Content?.ToString() switch
            {
                "None" => null,
                "Image" => "dotnet_bot.png",
                _ => null
            };
        }
    }

    private void OnFlyoutScrollModeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutVerticalScrollMode = rb.Content?.ToString() switch
            {
                "Auto" => ScrollMode.Auto,
                "Enabled" => ScrollMode.Enabled,
                "Disabled" => ScrollMode.Disabled,
                _ => ScrollMode.Auto
            };
        }
    }

    private void OnFlyoutHeaderChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            switch (rb.Content?.ToString())
            {
                case "None":
                    _viewModel.FlyoutHeader = null;
                    _viewModel.FlyoutHeaderTemplate = null;
                    break;
                case "Header":
                    _viewModel.FlyoutHeader = new Grid()
                    {
                        HeightRequest = 80,
                        BackgroundColor = Colors.Black,
                        AutomationId = "Header",
                        Children =
                        {
                            new Image()
                            {
                                Aspect = Aspect.AspectFill,
                                Source = "red.png",
                                Opacity = 0.6
                            },
                            new Label()
                            {
                                Margin = new Thickness(0, 40, 0, 0),
                                Text="Header",
                                TextColor=Colors.Black,
                                HorizontalOptions = LayoutOptions.Center,
                                FontAttributes=FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center
                            }
                        }
                    };
                    _viewModel.FlyoutHeaderTemplate = null;
                    break;
                case "HeaderTemplate":
                    _viewModel.FlyoutHeaderTemplate = new DataTemplate(() =>
                    {
                        return new Grid()
                        {
                            HeightRequest = 80,
                            BackgroundColor = Colors.Black,
                            AutomationId = "HeaderTemplate",
                            Children =
                            {
                                new Image()
                                {
                                    Aspect = Aspect.AspectFill,
                                    Source = "blue.png",
                                    Opacity = 0.6
                                },
                                new Label()
                                {
                                    Margin = new Thickness(0, 40, 0, 0),
                                    Text="HeaderTemplate",
                                    HorizontalOptions = LayoutOptions.Center,
                                    TextColor=Colors.Black,
                                    FontAttributes=FontAttributes.Bold,
                                    VerticalTextAlignment = TextAlignment.Center
                                }
                            }
                        };
                    });

                    _viewModel.FlyoutHeader = null;
                    break;
            }
        }
    }
    private void OnFlyoutFooterChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            switch (rb.Content?.ToString())
            {
                case "None":
                    _viewModel.FlyoutFooter = null;
                    _viewModel.FlyoutFooterTemplate = null;
                    break;
                case "Footer":
                    _viewModel.FlyoutFooter = new Grid()
                    {
                        Padding = 15,
                        HeightRequest = 60,
                        BackgroundColor = Colors.Red,
                        Children =
                        {
                            new Label
                            {
                                Text = "Footer",
                                AutomationId="Footer",
                                FontSize = 16,
                                TextColor = Colors.Black,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                            },
                        }
                    };
                    _viewModel.FlyoutFooterTemplate = null;
                    break;
                case "FooterTemplate":
                    _viewModel.FlyoutFooterTemplate = new DataTemplate(() =>
                    {
                        return new Grid()
                        {
                            Padding = 15,
                            BackgroundColor = Colors.Blue,
                            HeightRequest = 60,
                            Children =
                            {
                                new Label
                                {
                                    Text = "FooterTemplate",
                                    AutomationId = "FooterTemplate",
                                    FontSize = 16,
                                    FontAttributes = FontAttributes.Bold,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    TextColor = Colors.Black
                                }
                            }
                        };
                    });

                    _viewModel.FlyoutFooter = null;
                    break;
            }
        }
    }

    private void OnBackgroundColorChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutBackgroundColor = rb.Content?.ToString() switch
            {
                "Default" => Colors.White,
                "Blue" => Colors.LightBlue,
                _ => Colors.White
            };
        }
    }
    private void OnFlyoutIconChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutIcon = rb.Content?.ToString() switch
            {
                "None" => null,
                "coffee" => "coffee.png",
                _ => null
            };
        }
    }

    private void OnBackdropChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            switch (rb.Content?.ToString())
            {
                case "None":
                    _viewModel.FlyoutBackdrop = Brush.Default;
                    break;
                case "Gradient":
                    _viewModel.FlyoutBackdrop = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1),
                        GradientStops = new GradientStopCollection
        {
            new GradientStop { Color = Color.FromArgb("#8A2387"), Offset = 0.1f },
            new GradientStop { Color = Color.FromArgb("#E94057"), Offset = 0.6f },
            new GradientStop { Color = Color.FromArgb("#F27121"), Offset = 1.0f }
        }
                    };
                    break;
            }
        }
    }
    private void OnDisplayOptionChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !e.Value)
            return;
        if (sender is RadioButton rb)
        {
            _viewModel.FlyoutDisplayOptions = rb.Content?.ToString() switch
            {
                "AsSingleItem" => FlyoutDisplayOptions.AsSingleItem,
                "AsMultipleItems" => FlyoutDisplayOptions.AsMultipleItems,
                _ => FlyoutDisplayOptions.AsSingleItem
            };
        }
    }
    private void OnFlyoutItemVisibilityChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;

        _viewModel.FlyoutItemIsVisible = rb.Content?.ToString() == "True";
    }
    private void IsPresentedRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;

        _viewModel.FlyoutIsPresented = rb.Content?.ToString() == "True";
    }
    private void OnFlowDirectionChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton rb)
        {
            _viewModel.FlowDirection = rb.Content?.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        }
    }
    private void OnFlyoutItemTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !(sender is RadioButton rb) || !e.Value)
            return;

        switch (rb.Content?.ToString())
        {
            case "None":
                _viewModel.ItemTemplate = null;
                break;

            case "Basic":
                _viewModel.ItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Start,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.DarkBlue,
                        FontSize = 16,
                        Padding = new Thickness(15, 10),
                        Margin = new Thickness(5, 2)
                    };
                    label.SetBinding(Label.TextProperty, "Title");
                    return label;
                });
                break;
        }
    }
    private void OnMenuItemTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_viewModel == null || !(sender is RadioButton rb) || !e.Value)
            return;

        switch (rb.Content?.ToString())
        {
            case "None":
                _viewModel.MenuItemTemplate = null;
                break;

            case "Basic":
                _viewModel.MenuItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Start,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.DarkRed,
                        FontSize = 16,
                        Padding = new Thickness(15, 10),
                        Margin = new Thickness(5, 2)
                    };
                    label.SetBinding(Label.TextProperty, "Text");
                    return label;
                });
                break;
        }
    }
}
