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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reset flyout to default items when options page appears
        ResetToDefaultItems();
        // Clear any radio button selection since we removed "Default Items"
        _viewModel.SelectedFlyoutTemplate = null;
    }

    private void ResetToDefaultItems()
    {
        if (Application.Current.MainPage is not Shell shell)
            return;
        // Reset to default flyout items behavior
        shell.FlyoutContent = null;
        shell.FlyoutContentTemplate = null;
        // Ensure the FlyoutItems visibility is properly applied
        foreach (var item in shell.Items.OfType<FlyoutItem>())
        {
            item.FlyoutItemIsVisible = _viewModel.FlyoutItemIsVisible;
        }
    }
    private async void OnApplyClicked(object sender, EventArgs e)
    {
        if (Application.Current.MainPage is Shell shell)
        {
            // Force re-apply header/footer to Shell
            shell.FlyoutHeader = _viewModel.FlyoutHeader;
            shell.FlyoutHeaderTemplate = _viewModel.FlyoutHeaderTemplate;
            shell.FlyoutFooter = _viewModel.FlyoutFooter;
            shell.FlyoutFooterTemplate = _viewModel.FlyoutFooterTemplate;
            shell.FlyoutBackgroundImage = _viewModel.FlyoutBackgroundImage;
            shell.FlyoutBackgroundImageAspect = _viewModel.FlyoutBackgroundImageAspect;
            shell.FlyoutBackgroundColor = _viewModel.FlyoutBackgroundColor;
            shell.FlyoutHeaderBehavior = _viewModel.FlyoutHeaderBehavior;
            shell.FlyoutIsPresented = _viewModel.FlyoutIsPresented;
            shell.FlyoutBehavior = _viewModel.FlyoutBehavior;

            foreach (var item in shell.Items.OfType<FlyoutItem>())
            {
                item.FlyoutDisplayOptions = _viewModel.FlyoutDisplayOptions;
                item.FlyoutItemIsVisible = _viewModel.FlyoutItemIsVisible;
            }
        }

        UpdateFlyoutTemplate();
        await Navigation.PopAsync();
    }

    private void UpdateFlyoutTemplate()
    {
        if (Application.Current.MainPage is not Shell shell)
            return;

        switch (_viewModel.SelectedFlyoutTemplate)
        {
            case "FlyoutContent":
                shell.FlyoutContent = new StackLayout
                {
                    Padding = 10,
                    Children =
                {
                    new Label
                    {
                        Text = "FlyoutContent Applied",
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

                shell.FlyoutContentTemplate = null;
                break;


            case "FlyoutContentTemplate":
                shell.FlyoutContentTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Text = "FlyoutContentTemplate Applied",
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

                shell.FlyoutContent = null;
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
                "Collapse" => FlyoutHeaderBehavior.CollapseOnScroll,
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
                    _viewModel.FlyoutHeader = new VerticalStackLayout
                    {
                        Spacing = 5,
                        Padding = 15,
                        BackgroundColor = Colors.LightBlue,
                        Children =
                        {
                            new Label
                            {
                                Text = "Header",
                                FontSize = 16,
                                TextColor = Colors.Red,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center,
                            },
                        }
                    };
                    _viewModel.FlyoutHeaderTemplate = null;
                    break;
                case "HeaderTemplate":
                    _viewModel.FlyoutHeaderTemplate = new DataTemplate(() =>
                    {
                        return new Grid
                        {
                            Padding = 10,
                            HeightRequest = 40,
                            BackgroundColor = Colors.LightGrey,
                            Children =
                            {
                            new Label
                            {
                                Text = "HeaderTemplate",
                                HorizontalOptions = LayoutOptions.Center,
                                FontAttributes = FontAttributes.Bold,
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
                    _viewModel.FlyoutFooter = new VerticalStackLayout
                    {
                        Spacing = 5,
                        Padding = 15,
                        BackgroundColor = Colors.LightCoral,
                        Children =
                        {
                            new Label
                            {
                                Text = "Footer",
                                FontSize = 16,
                                TextColor = Colors.Red,
                                HorizontalOptions = LayoutOptions.Center,
                            },
                        }
                    };
                    _viewModel.FlyoutFooterTemplate = null;
                    break;
                case "FooterTemplate":
                    _viewModel.FlyoutFooterTemplate = new DataTemplate(() =>
                    {
                        return new Grid
                        {
                            Padding = 15,
                            BackgroundColor = Colors.LightGrey,
                            HeightRequest = 60,
                            Children =
                            {
                                new Label
                                {
                                    Text = "FooterTemplate",
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

    private void OnFlyoutTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is not RadioButton rb || !e.Value)
            return;
        // Update selected template in ViewModel
        _viewModel.SelectedFlyoutTemplate = rb.Content?.ToString();
    }

    private void IsEnabledRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton rb) || !rb.IsChecked)
            return;
        _viewModel.IsEnabled = rb.Content?.ToString() == "True";
    }

    private void OnFlowDirectionChanged(object sender, EventArgs e)
    {
        _viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
    }
}