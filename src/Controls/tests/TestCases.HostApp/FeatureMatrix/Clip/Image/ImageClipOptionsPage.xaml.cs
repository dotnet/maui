using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public partial class ImageClipOptionsPage : ContentPage
{
	private ImageClipViewModel _viewModel;
	public static string Base64EncodedImage = "iVBORw0KGgoAAAANSUhEUgAAA+gAAAPoCAYAAABNo9TkAAAYuUlEQVR4Xu3aQU7qUACF4cNLdwMDouxDwgpk4jYq23ACKyC4DzUdwHr6TEdOTaSFe78vuSvA9PS3d9b3faYFAAAA/Eu1AAAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAgCajY7O4rJMsUyaA7nien/IrYM8B7Dmzvu8zKmP+nGSfsgFsv0f9kEmBPQew5664Y8wB9kPAgD0vGGDPEejGHMCogz0HsOcCHWMOYNSx5wD2HIFuzAGMOthzAHsu0DHmAEYdew5gzxHoxhzAqIM9B7DnAh1jDmDUsecA2HOBbswBjDrYcwB7LtAx5gBGHXsOgD0X6MYcwKiDPQew5wIdYw5g1LHnANhzgW7MAYw62HMAey7QMeYARh17DoA9F+jGHMCogz0HsOcCHWMOYNSx5wDYc4FuzAGMOthzAHsu0DHmAEYdew6APRfoxhwAo449B7DnAh1jDmDUsecA2HOBbswBMOrYcwB7LtAx5gBGHXsOgD0X6MYcAKOOPQew5wIdYw5g1LHnANhzgW7MATDq2HMAey7QMeYARh17DoA9F+jGHACjjj0HsOcIdGMOYNSx5wDYc4FuzAEw6thzAOy5QDfmAEYdew6APRfo04/5urgxBzDq62DPAbDnAv3uLAOAZzv2vAAA9lygAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAABDoAAAAg0AEAAECgAwAAAAIdAAAAaFIjdikd0OYHwJ4D9hxf0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAAAg0AEAAACBDgAAAAIdAAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAAg3CoEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAAECT6wFYJnnKeKDdLC5t6gBtpsB7ki6TAnxBBxDnAPA0bByAQAfEOQCIdECgA4hzABDpgEAHxDkAiHRAoAOIcwAQ6YBAB8Q5AIh0QKADiHMAEOmAQAfEOQCIdECgA4hzABDpgEAHxDkAiHRAoAOIcwAQ6YBAB8Q5AIh0QKADiHMAEOmAQAfEOQCIdECgA4hzABDpgEAHxDkAiHRAoAOIcwAQ6YBAB8Q5AIh0QKADiHMAEOmAQAfEOQCI9MoAAh0Q5wAg0gGBDohzAECkAwIdEOcAINIBgQ6IcwBApAMCHRDnACDSAYEOiHMAQKQDAh0Q5wAg0gGBDohzAECkAwIdEOcAINIBgQ6IcwBApAMCHSg7zgHAlgMCHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAQJNyAMBXko/UhFWSh9wUABDoAPCR5C1V4e4DHQBccQcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACBDgAAAAh0AAAAEOgAAACAQAcAAACa1Ig2BQAA8A4H+IIOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAAQ6AAAACHQAAABAoHeZFgAAAFpNoB/P81OSbW4FAAAA26HVBHqVkX64mUgHAAAQ54f8mkAX6QAAAIhzgS7SAQAAxLlAF+kAAACIc4Eu0gEAAMS5QBfpAAAAiHOBLtIBAADEuUAX6QAAAIhzgS7SAQAAxLlAF+kAAACIc4Eu0gEAAMS5QEekAwAAiHOBLtIBAADEuUBHpAMAAIhzgS7SAQAAxLlAR6QDAACIc4Eu0gEAAMS5QEekAwAAiHOBLtIBAADEuUBHpAMAAIhzgS7SAQAAxLlAR6QDAACIc4Eu0gEAAMS5QEekAwAAiHOBLtIBAADEuUBHpAMAAIhzgS7SAQAAxLlAR6QDAACIc4Eu0gEAAMQ5s77vMy42i8s6yTLjgTZ1+krykZrwOZya8DicmrBK8pA67TIe6L7j/JQ/h0AH/xTqU6e34ZQEgJfh1Hkbc5YfAFfcAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOhL8AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAAKDJ6NgsLusky1wbsEpt+BxOTXgcDp7tdbxDvmY80B3P81NGxazv+4xKnD8n2ecaAHgbTk14Gc41ALD9jvRDJuWKO+IcAABgPzQMAl2cAwAAiHSBjjgHAAAQ6QJdnAMAAIh0gY44BwAAEOkCXZwDAACIdIGOOAcAABDpAl2cAwAAiHSBjjgHAAAQ6QJdnAMAAIh0gY44BwAAEOkCXZwDAACIdIGOOAcAABDpAl2cAwAAiHQEujgHAAAQ6QJdnAMAAIh0BLo4BwAAEOkCXZwDAACIdAS6OAcAABDpAl2cAwAAiHQEujgHAAAQ6QJdnAMAAIh0BLo4BwAAEOkCXZwDAAAg0gW6OAcAABDpAl2cAwAAINIFujgHAAAQ6QJdnAMAACDSBbo4BwAAEOkC/YbjfH1zcQ4AACDS1wK9PssUAAAAQKsJdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAgEAHAAAABDoAAAAIdAAAAECgAwAAAE1qxC61oc1tAgC8w+EdDl/QAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAQKADAAAAAh0AAAAEOgAAACDQAQAAgCblAIBVJoffHAAEOgA8DKdAAIAr7gAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAABAoAMAAIBABwAAAAQ6AAAACHQAAABAoAMAAIBABwAAAAQ6AAAACHQAAABAoAMAAIBABwAAAAQ6AAAACHQAAABAoAMAAIBABwAAAAQ68J7JAQC2HBDoQHfXww4A4rwLINABkQ4AiHNAoAMiHQDEOSDQAZEOAIhzQKADIh0AxDkg0AGRDgCIc0CgAyIdAMQ5INABkQ4AiHNAoAMiHQDEOSDQAZEOAIhzQKADIh0AxDkg0AFEOgCIc0CgAyIdAMQ5INABRDoAiHNAoAMi/XYBgDgHBDog0gFAnAMCHUCkA4A4BwQ6INIBQJwDAh1ApAOAOAcEOiDSAUCcAwIdQKQDgDgHBDog0gFAnAMCHUCkA4A4BwQ6INIBQJwDAh1ApAOAOAcEOiDSAUCcAwIdQKQDgDgHBDog0gFAnAMCHUCkA4A4BwQ6INIBQJwDNLkegG44U6BNnXbH8/w11WCzuLzW/PeeHwDwBR0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAAAh0AAAAQ6AAAACDQAQAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6AAAAIBABwAAAIEOAAAACHQAAAAQ6IQ7BQAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAA0qRFtCgAA9hwAX9ABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAINABAAAAgQ4AAAACHQAAABDoAAAAgEAHAAAAgQ4AAAAI9C7TAsCzHXsOgD0X6Mfz/JRkm1IAsB2e7dhzAOy5QL/LUT8UM+oAxvwgAOw5APZcoBt1AIw59hwAey7QjTqAMQd7DmDPBbpRB8CYY88BsOcC3agDGHOw5wD2XKAbdQCMOfYcAHsu0I06gDEHew5gzwW6UQfAmGPPAbDnAt2oAxhzsOcA9lygG3UAjDn2HAB7LtCNOoAxB3sOYM8FulEHwJhjzwGw5wLdqAMYc7DnAPZcoGPUAYw59hwAey7QjTqAMQd7DmDPBTpGHcCYY88BsOcC3agDGHOw5wD2XKBj1AGMOfYcAHsu0I06gDEHew5gzwU6Rh3AmGPPAbDnAt2oAxhzsOcA9lygY9QBjDn2vEYA9lygG3UAYw72HMCeC3SMOoAxB3sOYM8FulEHMOZgzwHsuUDHqAMYc7DnAPZcoBt1AGMO9hzAngt0jDqAMQd7DmDPBbpRBzDmYM8B7Dmzvu8zLjaLyzrJMmUC6L7H/JS7AfYcwJ4LdAAAAMAVdwAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAABDoAAAAgEAHAAAAgQ4AAAAIdAAAAOA/QHZ2FC6ZI88AAAAASUVORK5CYII=";

	public ImageClipOptionsPage(ImageClipViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void AspectRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			switch (rb.Content?.ToString())
			{
				case "AspectFit":
					_viewModel.Aspect = Aspect.AspectFit;
					break;
				case "AspectFill":
					_viewModel.Aspect = Aspect.AspectFill;
					break;
				case "Fill":
					_viewModel.Aspect = Aspect.Fill;
					break;
				case "Center":
					_viewModel.Aspect = Aspect.Center;
					break;
			}
		}
	}

	private void SourceTypeRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked)
		{
			string type = rb.Content?.ToString() ?? "";
			switch (type)
			{
				case "ClipImage":
					_viewModel.Source = ImageSource.FromFile("blue.png");
					break;

				case "File":
					_viewModel.Source = new FileImageSource
					{
						File = "dotnet_bot.png"
					};
					break;
				case "Uri":
					_viewModel.Source = new UriImageSource
					{
						Uri = new Uri("https://aka.ms/campus.jpg"),
						CachingEnabled = true,
						CacheValidity = TimeSpan.MaxValue // Effectively infinite cache
					};
					break;

				case "Stream":
					var imageBytes = Convert.FromBase64String(Base64EncodedImage);
					_viewModel.Source = Microsoft.Maui.Controls.ImageSource.FromStream(() => new MemoryStream(imageBytes));
					break;
				case "FontImage":
					_viewModel.Source = new FontImageSource
					{
						FontFamily = "Ion",
						Glyph = "\uf30c",
						Color = _viewModel.Color,
						Size = _viewModel.Size
					};
					break;
			}
		}
	}

	private void ClipRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && BindingContext is ImageClipViewModel vm && sender is RadioButton rb)
		{
			vm.Clip = rb.Content?.ToString() switch
			{
				"None" => null,
				"Rectangle" => new RectangleGeometry(new Rect(0, 275, 150, 100)),
				"Ellipse" => new EllipseGeometry(new Point(175, 275), 100, 65),
				"RoundRectangle" => new RoundRectangleGeometry(new CornerRadius(50), new Rect(0, 275, 150, 100)),
				"GeometryGroup" => new GeometryGroup
				{
					Children = new GeometryCollection
					{
						new EllipseGeometry(new Point(100, 300), 80, 50),
						new RectangleGeometry(new Rect(200, 250, 100, 150))
					}
				},
				_ => null
			};
		}
	}

	private void PathTypeRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && BindingContext is ImageClipViewModel vm && sender is RadioButton rb)
		{
			string pathType = rb.Content?.ToString() ?? "LineSegment";
			vm.Clip = CreatePathGeometry(pathType);
		}
	}

	private PathGeometry CreatePathGeometry(string pathType)
	{
		return pathType switch
		{
			// LineSegment: Draws straight lines between points
			"Line" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(150, 20),
						IsClosed = true,
						Segments = new PathSegmentCollection
						{
							new LineSegment { Point = new Point(280, 180) },
							new LineSegment { Point = new Point(20, 180) }
						}
					}
				}
			},
			// ArcSegment: Draws an elliptical arc between two points
			"Arc" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new ArcSegment
							{
								Point = new Point(290, 100),
								Size = new Size(140, 80),
								RotationAngle = 0,
								IsLargeArc = true,
								SweepDirection = SweepDirection.CounterClockwise
							}
						}
					}
				}
			},
			// BezierSegment: Draws a cubic Bezier curve (2 control points)
			"Bezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new BezierSegment
							{
								Point1 = new Point(100, 0),
								Point2 = new Point(200, 200),
								Point3 = new Point(290, 100)
							}
						}
					}
				}
			},
			// QuadraticBezierSegment: Draws a quadratic Bezier curve (1 control point)
			"QuadraticBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new QuadraticBezierSegment
							{
								Point1 = new Point(150, 50),
								Point2 = new Point(290, 100)
							}
						}
					}
				}
			},
			// PolyLineSegment: Draws a series of connected straight lines
			"PolyLine" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(150, 10),
						IsClosed = true,
						Segments = new PathSegmentCollection
						{
							new PolyLineSegment
							{
								Points = new PointCollection
								{
									new Point(180, 80),
									new Point(260, 80),
									new Point(200, 130),
									new Point(230, 200),
									new Point(150, 160),
									new Point(70, 200),
									new Point(100, 130),
									new Point(40, 80),
									new Point(120, 80)
								}
							}
						}
					}
				}
			},
			// PolyBezierSegment: Draws a series of connected cubic Bezier curves
			"PolyBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new PolyBezierSegment
							{
								Points = new PointCollection
								{
									// First Bezier curve (3 points: control1, control2, end)
									new Point(100, 50),
									new Point(150, 50),
									new Point(200, 100),
									// Second Bezier curve
									new Point(250, 150),
									new Point(200, 150),
									new Point(290, 100)
								}
							}
						}
					}
				}
			},
			// PolyQuadraticBezierSegment: Draws a series of connected quadratic Bezier curves
			"PolyQuadraticBezier" => new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = new Point(10, 100),
						IsClosed = false,
						Segments = new PathSegmentCollection
						{
							new PolyQuadraticBezierSegment
							{
								Points = new PointCollection
								{
									// First quadratic curve (2 points: control, end)
									new Point(100, 50),
									new Point(150, 100),
									// Second quadratic curve
									new Point(200, 150),
									new Point(290, 100)
								}
							}
						}
					}
				}
			},
			_ => new PathGeometry()
		};
	}

}