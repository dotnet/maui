namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.None, "30403Panoramic",
	"Image under WinUI does not respect VerticalOptions and HorizontalOptions with AspectFit (Panoramic)",
	PlatformAffected.UWP)]
public class Issue30403PanoramicImages : ContentPage
{
	const double PanoramicWidth = 800;
	const double PanoramicHeight = 200;

	public Issue30403PanoramicImages()
	{
		Title = "Panoramic Image Tests";

		Content = new ScrollView
		{
			AutomationId = "PanoramicScroll",
			Content = new StackLayout
			{
				Spacing = 20,
				Padding = 15,
				Children =
				{
					CreatePageHeader(),
					CreateAspectFitScenario(
						"1. Center Alignment",
						"Panoramic image should be centered without stretching.",
						"PanoramicCenter",
						"PanoramicCenterResult",
						200,
						Colors.LightBlue,
						LayoutOptions.Center,
						LayoutOptions.Center),
					CreateAspectFitScenario(
						"2. Start Alignment",
						"Panoramic image should align to the top-left corner.",
						"PanoramicStart",
						"PanoramicStartResult",
						200,
						Colors.LightGreen,
						LayoutOptions.Start,
						LayoutOptions.Start),
					CreateAspectFitScenario(
						"3. End Alignment",
						"Panoramic image should align to the bottom-right corner.",
						"PanoramicEnd",
						"PanoramicEndResult",
						200,
						Colors.LightCoral,
						LayoutOptions.End,
						LayoutOptions.End),
					CreateFillAlignmentTest(),
					CreateAspectFitScenario(
						"5. Constrained Width Test",
						"Panoramic image in a very narrow 100px container.",
						"PanoramicNarrow",
						"PanoramicNarrowResult",
						200,
						Colors.Plum,
						LayoutOptions.Center,
						LayoutOptions.Center,
						100),
					CreateAspectFitScenario(
						"6. Constrained Height Test",
						"Panoramic image in a very short 50px high container.",
						"PanoramicShort",
						"PanoramicShortResult",
						50,
						Colors.LightSeaGreen,
						LayoutOptions.Center,
						LayoutOptions.Center),
					CreateMultiplePanoramicTest(),
					CreateInteractiveTest()
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
					Text = "Panoramic Image Tests",
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Testing wide local images (800x200) with AspectFit and different layout options.",
					FontSize = 14,
					TextColor = Colors.Gray,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				}
			}
		};
	}

	View CreateAspectFitScenario(
		string title,
		string description,
		string imageAutomationId,
		string resultAutomationId,
		double containerHeight,
		Color containerColor,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions,
		double? containerWidth = null)
	{
		var image = CreatePanoramicImage(imageAutomationId, verticalOptions, horizontalOptions, Colors.Yellow);
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

		return new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = title,
					FontSize = 16,
					FontAttributes = FontAttributes.Bold
				},
				new Label
				{
					Text = description,
					FontSize = 12,
					TextColor = Colors.Gray
				},
				container,
				CreateAspectFitResultLabel(
					resultAutomationId,
					image,
					container,
					PanoramicWidth,
					PanoramicHeight,
					verticalOptions,
					horizontalOptions)
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
					Text = "Fill alignment should use the available layout slot while AspectFit preserves the drawn image ratio.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				new Grid
				{
					HeightRequest = 200,
					BackgroundColor = Colors.Orange,
					Children =
					{
						CreatePanoramicImage(
							"PanoramicFill",
							LayoutOptions.Fill,
							LayoutOptions.Fill,
							Colors.Yellow)
					}
				}
			}
		};
	}

	View CreateMultiplePanoramicTest()
	{
		var topLeft = CreatePanoramicCell(
			"MultiPanoramicTopLeft",
			"MultiPanoramicTopLeftResult",
			LayoutOptions.Start,
			LayoutOptions.Start,
			Colors.Red,
			0,
			0);
		var topRight = CreatePanoramicCell(
			"MultiPanoramicTopRight",
			"MultiPanoramicTopRightResult",
			LayoutOptions.Start,
			LayoutOptions.End,
			Colors.Blue,
			0,
			1);
		var bottomLeft = CreatePanoramicCell(
			"MultiPanoramicBottomLeft",
			"MultiPanoramicBottomLeftResult",
			LayoutOptions.End,
			LayoutOptions.Start,
			Colors.Green,
			1,
			0);
		var bottomRight = CreatePanoramicCell(
			"MultiPanoramicBottomRight",
			"MultiPanoramicBottomRightResult",
			LayoutOptions.End,
			LayoutOptions.End,
			Colors.Orange,
			1,
			1);

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
			},
			HeightRequest = 200,
			BackgroundColor = Colors.Wheat,
			Children =
			{
				topLeft.Container,
				topRight.Container,
				bottomLeft.Container,
				bottomRight.Container
			}
		};

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
					Text = "Different panoramic images with various alignments in a grid.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				grid,
				topLeft.Result,
				topRight.Result,
				bottomLeft.Result,
				bottomRight.Result
			}
		};
	}

	(Grid Container, Label Result) CreatePanoramicCell(
		string imageAutomationId,
		string resultAutomationId,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions,
		Color imageBackground,
		int row,
		int column)
	{
		var image = CreatePanoramicImage(imageAutomationId, verticalOptions, horizontalOptions, imageBackground);
		var container = new Grid
		{
			BackgroundColor = Colors.White,
			Children = { image }
		};

		Grid.SetRow(container, row);
		Grid.SetColumn(container, column);

		return (
			container,
			CreateAspectFitResultLabel(
				resultAutomationId,
				image,
				container,
				PanoramicWidth,
				PanoramicHeight,
				verticalOptions,
				horizontalOptions));
	}

	View CreateInteractiveTest()
	{
		var currentAlignment = LayoutOptions.Center;
		var image = CreatePanoramicImage(
			"InteractivePanoramic",
			currentAlignment,
			currentAlignment,
			Colors.Yellow);

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
					Text = "Tap buttons to change alignment dynamically.",
					FontSize = 12,
					TextColor = Colors.Gray
				},
				statusLabel,
				buttonStack,
				new Grid
				{
					HeightRequest = 200,
					BackgroundColor = Colors.Lavender,
					Children = { image }
				}
			}
		};
	}

	Image CreatePanoramicImage(
		string automationId,
		LayoutOptions verticalOptions,
		LayoutOptions horizontalOptions,
		Color backgroundColor)
	{
		return new Image
		{
			AutomationId = automationId,
			Source = CreatePanoramicImageSource(),
			Aspect = Aspect.AspectFit,
			VerticalOptions = verticalOptions,
			HorizontalOptions = horizontalOptions,
			BackgroundColor = backgroundColor
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

	static ImageSource CreatePanoramicImageSource()
	{
		var bytes = System.Convert.FromBase64String(PanoramicImageBase64);
		return ImageSource.FromStream(() => new System.IO.MemoryStream(bytes));
	}

	const string PanoramicImageBase64 =
		"iVBORw0KGgoAAAANSUhEUgAAAyAAAADICAYAAAAQj4UaAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAQdSURBVHhe7dcxAQAgAAQhk5jZxm+LmxgIwblvAwAAKAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAAZAQEAADICAgAAJAREAAAICMgAABARkAAAICMgAAAABkBAQAAMgICAABkBAQAAMgICAAAkBEQAAAgIyAAAEBGQAAAgIyAAAAAGQEBAAAyAgIAAGQEBAAAyAgIAACQERAAACAjIAAAQEZAAACAjIAAAAAZAQEAADICAgAARLYPOl9N25Nng+IAAAAASUVORK5CYII=";
}
