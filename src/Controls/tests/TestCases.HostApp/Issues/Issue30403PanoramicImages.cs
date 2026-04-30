namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.None, "30403Panoramic",
    "Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Panoramic)", 
    PlatformAffected.UWP)]
public class Issue30403PanoramicImages : ContentPage
{
    public Issue30403PanoramicImages()
    {
        Title = "Panoramic Image Tests";

        var scrollView = new ScrollView { AutomationId = "PanoramicScroll" };

        var mainStack = new StackLayout
        {
            Spacing = 20,
            Padding = 15,
            Children =
            {
                CreatePageHeader(),
                CreateCenterAlignmentTest(),
                CreateStartAlignmentTest(),
                CreateEndAlignmentTest(),
                CreateFillAlignmentTest(),
                CreateConstrainedWidthTest(),
                CreateConstrainedHeightTest(),
                CreateMultiplePanoramicTest(),
                CreateInteractiveTest()
            }
        };

        scrollView.Content = mainStack;
        Content = scrollView;
    }

    View CreatePageHeader()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Panoramic Image Tests",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Testing wide images (800x200 aspect ratio) with different layout options",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };
    }

    View CreateCenterAlignmentTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "1. Center Alignment",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkBlue
                },
                new Label
                {
                    Text = "Panoramic image should be centered both horizontally and vertically",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HeightRequest = 200,
                    BackgroundColor = Colors.LightBlue,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "PanoramicCenter",
                            Source = CreatePanoramicImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                }
            }
        };
    }

    View CreateStartAlignmentTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "2. Start Alignment",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkGreen
                },
                new Label
                {
                    Text = "Panoramic image should align to top-left corner",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HeightRequest = 200,
                    BackgroundColor = Colors.LightGreen,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "PanoramicStart",
                            Source = CreatePanoramicImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Start,
                            HorizontalOptions = LayoutOptions.Start,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                }
            }
        };
    }

    View CreateEndAlignmentTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "3. End Alignment",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkRed
                },
                new Label
                {
                    Text = "Panoramic image should align to bottom-right corner",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HeightRequest = 200,
                    BackgroundColor = Colors.LightCoral,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "PanoramicEnd",
                            Source = CreatePanoramicImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.End,
                            HorizontalOptions = LayoutOptions.End,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                }
            }
        };
    }

    View CreateFillAlignmentTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "4. Fill Alignment",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkOrange
                },
                new Label
                {
                    Text = "Panoramic image should use maximum available space while maintaining aspect ratio",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HeightRequest = 200,
                    BackgroundColor = Colors.Orange,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "PanoramicFill",
                            Source = CreatePanoramicImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Fill,
                            HorizontalOptions = LayoutOptions.Fill,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                }
            }
        };
    }

    View CreateConstrainedWidthTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "5. Constrained Width Test",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Purple
                },
                new Label
                {
                    Text = "Panoramic image in very narrow container (100px wide)",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Grid
                        {
                            WidthRequest = 100,
                            HeightRequest = 200,
                            BackgroundColor = Colors.Plum,
                            Children =
                            {
                                new Image
                                {
                                    AutomationId = "PanoramicNarrow",
                                    Source = CreatePanoramicImageSource(),
                                    Aspect = Aspect.AspectFit,
                                    VerticalOptions = LayoutOptions.Center,
                                    HorizontalOptions = LayoutOptions.Center,
                                    BackgroundColor = Colors.Yellow
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    View CreateConstrainedHeightTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "6. Constrained Height Test",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Teal
                },
                new Label
                {
                    Text = "Panoramic image in very short container (50px high)",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    HeightRequest = 50,
                    BackgroundColor = Colors.LightSeaGreen,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "PanoramicShort",
                            Source = CreatePanoramicImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                }
            }
        };
    }

    View CreateMultiplePanoramicTest()
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new() { Height = new GridLength(1, GridUnitType.Star) },
                new() { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new() { Width = new GridLength(1, GridUnitType.Star) },
                new() { Width = new GridLength(1, GridUnitType.Star) }
            },
            HeightRequest = 200,
            BackgroundColor = Colors.Wheat
        };

        var topLeft = new Image
        {
            AutomationId = "MultiPanoramicTopLeft",
            Source = CreatePanoramicImageSource(Colors.Red, "TL"),
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.White
        };
        Grid.SetRow(topLeft, 0);
        Grid.SetColumn(topLeft, 0);

        var topRight = new Image
        {
            AutomationId = "MultiPanoramicTopRight",
            Source = CreatePanoramicImageSource(Colors.Blue, "TR"),
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.End,
            BackgroundColor = Colors.White
        };
        Grid.SetRow(topRight, 0);
        Grid.SetColumn(topRight, 1);

        var bottomLeft = new Image
        {
            AutomationId = "MultiPanoramicBottomLeft",
            Source = CreatePanoramicImageSource(Colors.Green, "BL"),
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.White
        };
        Grid.SetRow(bottomLeft, 1);
        Grid.SetColumn(bottomLeft, 0);

        var bottomRight = new Image
        {
            AutomationId = "MultiPanoramicBottomRight",
            Source = CreatePanoramicImageSource(Colors.Orange, "BR"),
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.End,
            BackgroundColor = Colors.White
        };
        Grid.SetRow(bottomRight, 1);
        Grid.SetColumn(bottomRight, 1);

        grid.Children.Add(topLeft);
        grid.Children.Add(topRight);
        grid.Children.Add(bottomLeft);
        grid.Children.Add(bottomRight);

        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "7. Multiple Panoramic Images",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Brown
                },
                new Label
                {
                    Text = "Different panoramic images with various alignments in a grid",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                grid
            }
        };
    }

    View CreateInteractiveTest()
    {
        var currentAlignment = LayoutOptions.Center;
        var image = new Image
        {
            AutomationId = "InteractivePanoramic",
            Source = CreatePanoramicImageSource(),
            Aspect = Aspect.AspectFit,
            VerticalOptions = currentAlignment,
            HorizontalOptions = currentAlignment,
            BackgroundColor = Colors.Yellow
        };

        var statusLabel = new Label
        {
            Text = "Current Alignment: Center",
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.DarkBlue
        };

        var buttonStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 10,
            Children =
            {
                CreateAlignmentButton("Center", () => SetAlignment(LayoutOptions.Center)),
                CreateAlignmentButton("Start", () => SetAlignment(LayoutOptions.Start)),
                CreateAlignmentButton("End", () => SetAlignment(LayoutOptions.End)),
                CreateAlignmentButton("Fill", () => SetAlignment(LayoutOptions.Fill))
            }
        };

        void SetAlignment(LayoutOptions alignment)
        {
            currentAlignment = alignment;
            image.VerticalOptions = alignment;
            image.HorizontalOptions = alignment;
            statusLabel.Text = $"Current Alignment: {alignment}";
        }

        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "8. Interactive Alignment Test",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Indigo
                },
                new Label
                {
                    Text = "Tap buttons to change alignment dynamically",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                statusLabel,
                buttonStack,
                new Grid { HeightRequest = 200, BackgroundColor = Colors.Lavender, Children = { image } }
            }
        };
    }

    Button CreateAlignmentButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            BackgroundColor = Colors.LightBlue,
            TextColor = Colors.Black,
            CornerRadius = 5,
            Padding = new Thickness(10, 5)
        };

        button.Clicked += (s, e) => action();
        return button;
    }

    ImageSource CreatePanoramicImageSource(Color color = null, string label = null)
    {
        var displayLabel = label ?? "Panoramic+800x200";
        string hexColor = color is null ? "4A90E2" : color.ToHex()[1..]; // Default to nice blue

        return ImageSource.FromUri(
            new Uri($"https://dummyimage.com/800x200/{hexColor}/ffffff.png&text={displayLabel}"));
    }
}