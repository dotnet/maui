namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.None, "30403Small",
	"Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Small Images)", PlatformAffected.UWP)]
public class Issue30403SmallImages : ContentPage
{
    public Issue30403SmallImages()
    {
        Title = "Small Image Tests";
        
        var scrollView = new ScrollView { AutomationId = "SmallScroll" };

        var mainStack = new StackLayout
        {
            Spacing = 20,
            Padding = 15,
            Children =
            {
                CreatePageHeader(),
                CreateSmallImageInLargeContainer(),
                CreateMultipleSmallImages(),
                CreateSmallImageAlignmentGrid(),
                CreateConstrainedSmallImages(),
                CreateTinyImageTest(),
                CreateSmallImageWithFillTest(),
                CreateComparisonTest()
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
                    Text = "Small Image Tests",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Testing small images (50x50) that should NOT exceed intrinsic size with Center/Start/End alignment",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text = "⚠️ Key Test: Small images should remain small, not stretch to fill large containers",
                    FontSize = 12,
                    TextColor = Colors.Red,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold
                }
            }
        };
    }

    View CreateSmallImageInLargeContainer()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "1. Small Image in Large Container (Center)",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkBlue
                },
                new Label
                {
                    Text = "50x50 image in 400x300 container - should remain ~50x50 and be centered",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    WidthRequest = 400,
                    HeightRequest = 300,
                    BackgroundColor = Colors.LightBlue,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "SmallImageCenter",
                            Source = CreateSmallImageSource(Colors.Red, "50x50"),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            BackgroundColor = Colors.Yellow
                        }
                    }
                },
                new Label
                {
                    Text = "✓ Expected: Small yellow square in center of blue container",
                    FontSize = 12,
                    TextColor = Colors.Green,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }

    View CreateMultipleSmallImages()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "2. Multiple Small Images with Different Alignments",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkGreen
                },
                new Label
                {
                    Text = "Start, Center, End alignments in large containers",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new StackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        // Start alignment
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "Start Alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold },
                                new Grid
                                {
                                    HeightRequest = 120,
                                    BackgroundColor = Colors.LightGreen,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageStart",
                                            Source = CreateSmallImageSource(Colors.Blue, "START"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.Start,
                                            HorizontalOptions = LayoutOptions.Start,
                                            BackgroundColor = Colors.Orange
                                        }
                                    }
                                }
                            }
                        },
                        
                        // End alignment
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "End Alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold },
                                new Grid
                                {
                                    HeightRequest = 120,
                                    BackgroundColor = Colors.LightCoral,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageEnd",
                                            Source = CreateSmallImageSource(Colors.Purple, "END"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.End,
                                            HorizontalOptions = LayoutOptions.End,
                                            BackgroundColor = Colors.Orange
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    View CreateSmallImageAlignmentGrid()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "3. Small Image Alignment Grid (3x3)",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkRed
                },
                new Label
                {
                    Text = "All 9 alignment combinations - each image should remain small",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new() { Height = new GridLength(100, GridUnitType.Absolute) },
                        new() { Height = new GridLength(100, GridUnitType.Absolute) },
                        new() { Height = new GridLength(100, GridUnitType.Absolute) }
                    },
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new() { Width = new GridLength(1, GridUnitType.Star) },
                        new() { Width = new GridLength(1, GridUnitType.Star) },
                        new() { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    BackgroundColor = Colors.LightGray,
                    RowSpacing = 2,
                    ColumnSpacing = 2,
                    Children =
                    {
                        // Row 0
                        CreateGridImage("SmallGridStartStart", LayoutOptions.Start, LayoutOptions.Start, "SS", Colors.Red, 0, 0),
                        CreateGridImage("SmallGridStartCenter", LayoutOptions.Start, LayoutOptions.Center, "SC", Colors.Orange, 0, 1),
                        CreateGridImage("SmallGridStartEnd", LayoutOptions.Start, LayoutOptions.End, "SE", Colors.Yellow, 0, 2),
                        
                        // Row 1
                        CreateGridImage("SmallGridCenterStart", LayoutOptions.Center, LayoutOptions.Start, "CS", Colors.Green, 1, 0),
                        CreateGridImage("SmallGridCenterCenter", LayoutOptions.Center, LayoutOptions.Center, "CC", Colors.Blue, 1, 1),
                        CreateGridImage("SmallGridCenterEnd", LayoutOptions.Center, LayoutOptions.End, "CE", Colors.Purple, 1, 2),
                        
                        // Row 2
                        CreateGridImage("SmallGridEndStart", LayoutOptions.End, LayoutOptions.Start, "ES", Colors.Cyan, 2, 0),
                        CreateGridImage("SmallGridEndCenter", LayoutOptions.End, LayoutOptions.Center, "EC", Colors.Magenta, 2, 1),
                        CreateGridImage("SmallGridEndEnd", LayoutOptions.End, LayoutOptions.End, "EE", Colors.Brown, 2, 2)
                    }
                }
            }
        };
    }

    View CreateConstrainedSmallImages()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "4. Small Images in Constrained Containers",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.DarkOrange
                },
                new Label
                {
                    Text = "Small images in containers smaller than the image",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 15,
                    Children =
                    {
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "30x30 Container:", FontSize = 10, HorizontalOptions = LayoutOptions.Center },
                                new Grid
                                {
                                    WidthRequest = 30,
                                    HeightRequest = 30,
                                    BackgroundColor = Colors.Red,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageConstrained30",
                                            Source = CreateSmallImageSource(Colors.White, "S"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.Center,
                                            HorizontalOptions = LayoutOptions.Center,
                                            BackgroundColor = Colors.Transparent
                                        }
                                    }
                                }
                            }
                        },
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "40x40 Container:", FontSize = 10, HorizontalOptions = LayoutOptions.Center },
                                new Grid
                                {
                                    WidthRequest = 40,
                                    HeightRequest = 40,
                                    BackgroundColor = Colors.Blue,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageConstrained40",
                                            Source = CreateSmallImageSource(Colors.White, "M"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.Center,
                                            HorizontalOptions = LayoutOptions.Center,
                                            BackgroundColor = Colors.Transparent
                                        }
                                    }
                                }
                            }
                        },
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "60x60 Container:", FontSize = 10, HorizontalOptions = LayoutOptions.Center },
                                new Grid
                                {
                                    WidthRequest = 60,
                                    HeightRequest = 60,
                                    BackgroundColor = Colors.Green,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageConstrained60",
                                            Source = CreateSmallImageSource(Colors.White, "L"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.Center,
                                            HorizontalOptions = LayoutOptions.Center,
                                            BackgroundColor = Colors.Transparent
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    View CreateTinyImageTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "5. Tiny Image Test (10x10)",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Purple
                },
                new Label
                {
                    Text = "Extremely small image in large container",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new Grid
                {
                    WidthRequest = 300,
                    HeightRequest = 200,
                    BackgroundColor = Colors.Plum,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = "TinyImage",
                            Source = CreateTinyImageSource(),
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            BackgroundColor = Colors.White
                        }
                    }
                },
                new Label
                {
                    Text = "✓ Expected: Tiny white square (may be hard to see) in center",
                    FontSize = 12,
                    TextColor = Colors.Green,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }

    View CreateSmallImageWithFillTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "6. Small Image with Fill Alignment",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Teal
                },
                new Label
                {
                    Text = "Fill alignment should use available space (different from Center)",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "Fill Alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold },
                                new Grid
                                {
                                    WidthRequest = 150,
                                    HeightRequest = 100,
                                    BackgroundColor = Colors.LightSeaGreen,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageFill",
                                            Source = CreateSmallImageSource(Colors.Red, "FILL"),
                                            Aspect = Aspect.AspectFit,
                                            VerticalOptions = LayoutOptions.Fill,
                                            HorizontalOptions = LayoutOptions.Fill,
                                            BackgroundColor = Colors.Yellow
                                        }
                                    }
                                }
                            }
                        },
                        new StackLayout
                        {
                            Children =
                            {
                                new Label { Text = "Center Alignment:", FontSize = 12, FontAttributes = FontAttributes.Bold },
                                new Grid
                                {
                                    WidthRequest = 150,
                                    HeightRequest = 100,
                                    BackgroundColor = Colors.LightSeaGreen,
                                    Children =
                                    {
                                        new Image
                                        {
                                            AutomationId = "SmallImageCenterCompare",
                                            Source = CreateSmallImageSource(Colors.Red, "CTR"),
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
                }
            }
        };
    }

    View CreateComparisonTest()
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "7. Size Comparison Test",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Brown
                },
                new Label
                {
                    Text = "Different image sizes in identical containers",
                    FontSize = 12,
                    TextColor = Colors.Gray
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        CreateComparisonContainer("10x10", CreateTinyImageSource(), "ComparisonTiny"),
                        CreateComparisonContainer("50x50", CreateSmallImageSource(Colors.Blue, "50"), "ComparisonSmall"),
                        CreateComparisonContainer("100x100", CreateMediumImageSource(), "ComparisonMedium"),
                        CreateComparisonContainer("200x200", CreateLargeSquareImageSource(), "ComparisonLarge")
                    }
                }
            }
        };
    }

    View CreateComparisonContainer(string label, ImageSource source, string automationId)
    {
        return new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = label,
                    FontSize = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                },
                new Grid
                {
                    WidthRequest = 80,
                    HeightRequest = 80,
                    BackgroundColor = Colors.Wheat,
                    Children =
                    {
                        new Image
                        {
                            AutomationId = automationId,
                            Source = source,
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            BackgroundColor = Colors.White
                        }
                    }
                }
            }
        };
    }

    Image CreateGridImage(string automationId, LayoutOptions vertical, LayoutOptions horizontal, 
	    string label, Color color, int row, int col)
    {
	    var image = new Image
	    {
		    AutomationId = automationId,
		    Source = CreateSmallImageSource(color, label),
		    Aspect = Aspect.AspectFit,
		    VerticalOptions = vertical,
		    HorizontalOptions = horizontal,
		    BackgroundColor = Colors.White
	    };

	    Grid.SetRow(image, row);
	    Grid.SetColumn(image, col);

	    return image;
    }

    ImageSource CreateSmallImageSource(Color color, string label)
    {
        return ImageSource.FromUri(
	        new Uri("https://dummyimage.com/50x50/000000/ffffff.png&text=Small+50x50"));
    }

    ImageSource CreateTinyImageSource()
    {
        return ImageSource.FromUri(
	        new Uri("https://dummyimage.com/10x10/000000/ffffff.png&text=Tiny+10x10"));
    }

    ImageSource CreateMediumImageSource()
    {
	    return ImageSource.FromUri(
		    new Uri("https://dummyimage.com/100x100/000000/ffffff.png&text=Medium+100x100"));
    }

    ImageSource CreateLargeSquareImageSource()
    {
	    return ImageSource.FromUri(
		    new Uri("https://dummyimage.com/200x200/000000/ffffff.png&text=LargeSquare+200x200"));
    }
}