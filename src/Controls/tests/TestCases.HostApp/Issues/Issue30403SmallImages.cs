namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.None, "30403Small",
	"Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Small Images)",
	PlatformAffected.UWP)]
public class Issue30403SmallImages : ContentPage
{
	public Issue30403SmallImages()
	{
		Title = "Small Image Tests";

		Content = new ScrollView
		{
			AutomationId = "SmallScroll",
			Content = new StackLayout
			{
				Spacing = 20,
				Padding = 15,
				Children =
				{
					CreatePageHeader(),
					CreateImageScenario(
						"1. Small Image in Large Container (Center)",
						"50x50 image in 400x300 container should stay near 50x50 and be centered.",
						"SmallImageCenter",
						"SmallImageCenterResult",
						50,
						50,
						400,
						300,
						Colors.LightBlue,
						Colors.Yellow,
						LayoutOptions.Center,
						LayoutOptions.Center),
					CreateMultipleSmallImages(),
					CreateSmallImageAlignmentGrid(),
					CreateConstrainedSmallImages(),
					CreateImageScenario(
						"5. Tiny Image Test (10x10)",
						"10x10 image in a large container should remain tiny and centered.",
						"TinyImage",
						"TinyImageResult",
						10,
						10,
						300,
						200,
						Colors.Plum,
						Colors.White,
						LayoutOptions.Center,
						LayoutOptions.Center),
					CreateSmallImageWithFillTest(),
					CreateComparisonTest()
				}
			}
		};
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
					Text = "Testing local square images that should not exceed their intrinsic size with AspectFit.",
					FontSize = 14,
					TextColor = Colors.Gray,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Label
				{
					Text = "Key test: small images should remain small, not stretch to fill large containers.",
					FontSize = 12,
					TextColor = Colors.Red,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					FontAttributes = FontAttributes.Bold
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
					Text = "Start and End alignments in large containers.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				new StackLayout
				{
					Spacing = 10,
					Children =
					{
						CreateImageScenario(
							"Start Alignment",
							"50x50 image should stay at the top-left.",
							"SmallImageStart",
							"SmallImageStartResult",
							50,
							50,
							null,
							120,
							Colors.LightGreen,
							Colors.Orange,
							LayoutOptions.Start,
							LayoutOptions.Start),
						CreateImageScenario(
							"End Alignment",
							"50x50 image should stay at the bottom-right.",
							"SmallImageEnd",
							"SmallImageEndResult",
							50,
							50,
							null,
							120,
							Colors.LightCoral,
							Colors.Orange,
							LayoutOptions.End,
							LayoutOptions.End)
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
					Text = "All 9 alignment combinations should keep each image small.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				new Grid
				{
					RowDefinitions =
					{
						new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
						new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
						new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) }
					},
					ColumnDefinitions =
					{
						new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
						new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
					},
					BackgroundColor = Colors.LightGray,
					RowSpacing = 2,
					ColumnSpacing = 2,
					Children =
					{
						CreateGridImage("SmallGridStartStart", LayoutOptions.Start, LayoutOptions.Start, Colors.Red, 0, 0),
						CreateGridImage("SmallGridStartCenter", LayoutOptions.Start, LayoutOptions.Center, Colors.Orange, 0, 1),
						CreateGridImage("SmallGridStartEnd", LayoutOptions.Start, LayoutOptions.End, Colors.Yellow, 0, 2),
						CreateGridImage("SmallGridCenterStart", LayoutOptions.Center, LayoutOptions.Start, Colors.Green, 1, 0),
						CreateGridImage("SmallGridCenterCenter", LayoutOptions.Center, LayoutOptions.Center, Colors.Blue, 1, 1),
						CreateGridImage("SmallGridCenterEnd", LayoutOptions.Center, LayoutOptions.End, Colors.Purple, 1, 2),
						CreateGridImage("SmallGridEndStart", LayoutOptions.End, LayoutOptions.Start, Colors.Cyan, 2, 0),
						CreateGridImage("SmallGridEndCenter", LayoutOptions.End, LayoutOptions.Center, Colors.Magenta, 2, 1),
						CreateGridImage("SmallGridEndEnd", LayoutOptions.End, LayoutOptions.End, Colors.Brown, 2, 2)
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
					Text = "50x50 images in containers smaller than the source image.",
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
						CreateImageScenario(
							"30x30 Container",
							string.Empty,
							"SmallImageConstrained30",
							"SmallImageConstrained30Result",
							50,
							50,
							30,
							30,
							Colors.Red,
							Colors.Transparent,
							LayoutOptions.Center,
							LayoutOptions.Center),
						CreateImageScenario(
							"40x40 Container",
							string.Empty,
							"SmallImageConstrained40",
							"SmallImageConstrained40Result",
							50,
							50,
							40,
							40,
							Colors.Blue,
							Colors.Transparent,
							LayoutOptions.Center,
							LayoutOptions.Center),
						CreateImageScenario(
							"60x60 Container",
							string.Empty,
							"SmallImageConstrained60",
							"SmallImageConstrained60Result",
							50,
							50,
							60,
							60,
							Colors.Green,
							Colors.Transparent,
							LayoutOptions.Center,
							LayoutOptions.Center)
					}
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
					Text = "Fill alignment uses the available layout slot and Center remains intrinsic-sized.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Spacing = 10,
					Children =
					{
						CreateFillComparisonImage("Fill Alignment", "SmallImageFill", LayoutOptions.Fill, LayoutOptions.Fill),
						CreateFillComparisonImage("Center Alignment", "SmallImageCenterCompare", LayoutOptions.Center, LayoutOptions.Center)
					}
				}
			}
		};
	}

	View CreateComparisonTest()
	{
		var tiny = CreateComparisonContainer("10x10", CreateTinyImageSource(), "ComparisonTiny", 10);
		var small = CreateComparisonContainer("50x50", CreateSmallImageSource(), "ComparisonSmall", 50);
		var medium = CreateComparisonContainer("100x100", CreateMediumImageSource(), "ComparisonMedium", 100);
		var large = CreateComparisonContainer("200x200", CreateLargeSquareImageSource(), "ComparisonLarge", 200);

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
					Text = "Different image sizes in identical 80x80 containers.",
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
						tiny.Container,
						small.Container,
						medium.Container,
						large.Container
					}
				},
				tiny.Result,
				small.Result,
				medium.Result,
				large.Result
			}
		};
	}

	View CreateImageScenario(
		string title,
		string description,
		string imageAutomationId,
		string resultAutomationId,
		double sourceWidth,
		double sourceHeight,
		double? containerWidth,
		double containerHeight,
		Color containerColor,
		Color imageBackground,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions)
	{
		var image = CreateImage(imageAutomationId, CreateImageSource(sourceWidth, sourceHeight), verticalOptions, horizontalOptions, imageBackground);
		var container = new Grid
		{
			AutomationId = $"{imageAutomationId}Container",
			HeightRequest = containerHeight,
			BackgroundColor = containerColor,
			HorizontalOptions = containerWidth.HasValue ? LayoutOptions.Center : LayoutOptions.Fill,
			Children = { image }
		};

		if (containerWidth.HasValue)
			container.WidthRequest = containerWidth.Value;

		var stack = new StackLayout();

		stack.Children.Add(new Label
		{
			Text = title,
			FontSize = 16,
			FontAttributes = FontAttributes.Bold
		});

		if (!string.IsNullOrEmpty(description))
		{
			stack.Children.Add(new Label
			{
				Text = description,
				FontSize = 12,
				TextColor = Colors.Gray
			});
		}

		stack.Children.Add(container);
		stack.Children.Add(CreateAspectFitResultLabel(
			resultAutomationId,
			image,
			container,
			sourceWidth,
			sourceHeight,
			verticalOptions,
			horizontalOptions));

		return stack;
	}

	View CreateFillComparisonImage(
		string label,
		string automationId,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions)
	{
		return new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = label,
					FontSize = 12,
					FontAttributes = FontAttributes.Bold
				},
				new Grid
				{
					WidthRequest = 150,
					HeightRequest = 100,
					BackgroundColor = Colors.LightSeaGreen,
					Children =
					{
						CreateImage(
							automationId,
							CreateSmallImageSource(),
							verticalOptions,
							horizontalOptions,
							Colors.Yellow)
					}
				}
			}
		};
	}

	(StackLayout Container, Label Result) CreateComparisonContainer(
		string label,
		ImageSource source,
		string automationId,
		double sourceSize)
	{
		var image = CreateImage(automationId, source, LayoutOptions.Center, LayoutOptions.Center, Colors.White);
		var container = new Grid
		{
			WidthRequest = 80,
			HeightRequest = 80,
			BackgroundColor = Colors.Wheat,
			Children = { image }
		};

		return (
			new StackLayout
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
					container
				}
			},
			CreateAspectFitResultLabel(
				$"{automationId}Result",
				image,
				container,
				sourceSize,
				sourceSize,
				LayoutOptions.Center,
				LayoutOptions.Center));
	}

	Image CreateGridImage(
		string automationId,
		LayoutOptions vertical,
		LayoutOptions horizontal,
		Color color,
		int row,
		int column)
	{
		var image = CreateImage(automationId, CreateSmallImageSource(), vertical, horizontal, color);

		Grid.SetRow(image, row);
		Grid.SetColumn(image, column);

		return image;
	}

	Image CreateImage(
		string automationId,
		ImageSource source,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions,
		Color backgroundColor)
	{
		return new Image
		{
			AutomationId = automationId,
			Source = source,
			Aspect = Aspect.AspectFit,
			VerticalOptions = verticalOptions,
			HorizontalOptions = horizontalOptions,
			BackgroundColor = backgroundColor
		};
	}

	Label CreateAspectFitResultLabel(
		string automationId,
		Image image,
		VisualElement container,
		double sourceWidth,
		double sourceHeight,
		LayoutOptions expectedVertical,
		LayoutOptions expectedHorizontal)
	{
		var resultLabel = new Label
		{
			AutomationId = automationId,
			Text = "PENDING",
			FontSize = 12
		};

		void UpdateResult()
		{
			if (container.Width <= 0 || container.Height <= 0 || image.Width <= 0 || image.Height <= 0)
				return;

			var scale = System.Math.Min(1, System.Math.Min(container.Width / sourceWidth, container.Height / sourceHeight));
			var expectedWidth = sourceWidth * scale;
			var expectedHeight = sourceHeight * scale;

			var fitsSize = IsClose(image.Width, expectedWidth) && IsClose(image.Height, expectedHeight);
			var alignsHorizontally = Aligns(image.X, image.Width, container.Width, expectedHorizontal);
			var alignsVertically = Aligns(image.Y, image.Height, container.Height, expectedVertical);

			resultLabel.Text = fitsSize && alignsHorizontally && alignsVertically
				? $"PASS width={image.Width:F1} height={image.Height:F1} x={image.X:F1} y={image.Y:F1}"
				: $"FAIL expected={expectedWidth:F1}x{expectedHeight:F1} actual={image.Width:F1}x{image.Height:F1} x={image.X:F1} y={image.Y:F1} container={container.Width:F1}x{container.Height:F1}";
		}

		image.SizeChanged += (_, _) => UpdateResult();
		container.SizeChanged += (_, _) => UpdateResult();

		return resultLabel;
	}

	static bool Aligns(double actualStart, double actualSize, double containerSize, LayoutOptions expected)
	{
		if (expected == LayoutOptions.Start)
			return IsClose(actualStart, 0);

		if (expected == LayoutOptions.Center)
			return IsClose(actualStart + (actualSize / 2), containerSize / 2);

		if (expected == LayoutOptions.End)
			return IsClose(actualStart + actualSize, containerSize);

		if (expected == LayoutOptions.Fill)
			return IsClose(actualSize, containerSize);

		return true;
	}

	static bool IsClose(double actual, double expected)
	{
		const double tolerance = 4;
		return System.Math.Abs(actual - expected) <= tolerance;
	}

	static ImageSource CreateImageSource(double sourceWidth, double sourceHeight)
	{
		if (sourceWidth == 10 && sourceHeight == 10)
			return CreateImageSource(Tiny10ImageBase64);

		if (sourceWidth == 100 && sourceHeight == 100)
			return CreateImageSource(Square100ImageBase64);

		if (sourceWidth == 200 && sourceHeight == 200)
			return CreateImageSource(Square200ImageBase64);

		return CreateImageSource(Square50ImageBase64);
	}

	static ImageSource CreateSmallImageSource() => CreateImageSource(Square50ImageBase64);

	static ImageSource CreateTinyImageSource() => CreateImageSource(Tiny10ImageBase64);

	static ImageSource CreateMediumImageSource() => CreateImageSource(Square100ImageBase64);

	static ImageSource CreateLargeSquareImageSource() => CreateImageSource(Square200ImageBase64);

	static ImageSource CreateImageSource(string base64Png)
	{
		var bytes = System.Convert.FromBase64String(base64Png);
		return ImageSource.FromStream(() => new System.IO.MemoryStream(bytes));
	}

	const string Square50ImageBase64 =
		"iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABQSURBVGhD7c9BDQAwEASh9W/6amPS8EAAu+1+IFIjUiNSI1IjUiNSI1IjUiNSI1IjUiNSI1IjUiNSI1IjUiNSI1IjUiNSI1IjUiNS80lk9wCwM3WW+Ab2EgAAAABJRU5ErkJggg==";
	const string Tiny10ImageBase64 =
		"iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAUSURBVChTY/hPJBhViBdQW+H//wAj+46A3TfWvAAAAABJRU5ErkJggg==";
	const string Square100ImageBase64 =
		"iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD9SURBVHhe7cihAQAwEISw33/p6wJ4KhAxubstP8GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezHgw48GMBzMezDi2B3P91mR8J4VmAAAAAElFTkSuQmCC";
	const string Square200ImageBase64 =
		"iVBORw0KGgoAAAANSUhEUgAAAMgAAADICAYAAACtWK6eAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHhSURBVHhe7cixDYAwAASxjM7mAenbYwMXbnzOcy7wIxOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygckEJhOYTGAygc+5LwiV0WhwA9t2AAAAAElFTkSuQmCC";
}
