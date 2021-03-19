using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10623, "Tap Gesture not working on iOS [Bug]", PlatformAffected.iOS)]
	public class Issue10623 : TestContentPage
	{
		public Issue10623()
		{
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "Shapes_Experimental" });

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Tap the rating control. If you can modify the number of selected stars, the test has passed."
			};

			var rating = new RatingControl
			{
				Size = 30,
				Max = 5,
				FillColor = Brush.Gold,
				StrokeColor = Brush.Silver,
				StrokeThickness = 2,
				HorizontalOptions = LayoutOptions.Center
			};

			layout.Children.Add(instructions);
			layout.Children.Add(rating);

			Content = layout;
		}

		protected override void Init()
		{
			Title = "Issue 10623";
		}
	}

	public class RatingControl : StackLayout
	{
		public RatingControl()
		{
			Orientation = StackOrientation.Horizontal;
		}

		public static BindableProperty RatingProperty = BindableProperty.Create(nameof(Rating), typeof(double), typeof(RatingControl), 0.0, BindingMode.OneWay, propertyChanged: (bindable, newValue, oldValue) =>
		{
			var control = (RatingControl)bindable;

			if (newValue != oldValue)
			{
				control.Draw();
			}
		});

		readonly List<Point> _originalFullStarPoints = new List<Point>()
		{
			new Point(96,1.12977573),
			new Point(66.9427701,60.0061542),
			new Point(1.96882894,69.4474205),
			new Point(48.9844145,115.27629),
			new Point(37.8855403,179.987692),
			new Point(96,149.435112),
			new Point(154.11446,179.987692),
			new Point(143.015586,115.27629),
			new Point(190.031171,69.4474205),
			new Point(125.05723,60.0061542),
			new Point(96,1.12977573),
		};

		readonly List<Point> _originalHalfStarPoints = new List<Point>()
		{
			new Point(96,1.12977573),
			new Point(66.9427701,60.0061542),
			new Point(1.96882894,69.4474205),
			new Point(48.9844145,115.27629),
			new Point(37.8855403,179.987692),
			new Point(96,149.435112),
			new Point(96,1.12977573)
		};

		readonly PointCollection _fullStarPoints = new PointCollection();
		readonly PointCollection _halfStarPoints = new PointCollection();

		double _ratio;

		private void Draw()
		{
			Children.Clear();

			var newRatio = Size / 200;

			if (newRatio != _ratio)
			{
				_ratio = newRatio;

				CalculatePoints(_fullStarPoints, _originalFullStarPoints);
				CalculatePoints(_halfStarPoints, _originalHalfStarPoints);
			}


			for (var i = 1; i <= Max; i++)
			{
				if (Rating >= i)
				{
					Children.Add(GetFullStar());
				}
				else if (Rating > i - 1)
				{
					Children.Add(GetHalfStar());
				}
				else
				{
					Children.Add(GetEmptyStar());
				}
			}

			UpdateTapGestureRecognizers();
		}

		private void CalculatePoints(PointCollection calculated, List<Point> original)
		{
			calculated.Clear();

			foreach (var point in original)
			{
				var x = point.X * _ratio;
				var y = point.Y * _ratio;

				var p = new Point(x, y);

				calculated.Add(p);
			}
		}

		private Polygon GetFullStar()
		{
			var fullStar = new Polygon()
			{
				Points = _fullStarPoints,
				Fill = FillColor,
				StrokeThickness = StrokeThickness,
				Stroke = StrokeColor
			};

			return fullStar;
		}

		private Grid GetHalfStar()
		{
			var grid = new Grid();

			var halfStar = new Polygon()
			{
				Points = _halfStarPoints,
				Fill = _fillColor,
				Stroke = Brush.Transparent,
				StrokeThickness = 0,
			};

			var emptyStar = new Polygon()
			{
				Points = _fullStarPoints,
				StrokeThickness = StrokeThickness,
				Stroke = StrokeColor
			};

			grid.Children.Add(halfStar);
			grid.Children.Add(emptyStar);

			return grid;
		}

		private Polygon GetEmptyStar()
		{
			var emptyStar = new Polygon()
			{
				Points = _fullStarPoints,
				StrokeThickness = StrokeThickness,
				Stroke = StrokeColor
			};

			return emptyStar;
		}

		private void Set<T>(ref T field, T newValue)
		{
			if (!EqualityComparer<T>.Default.Equals(field, newValue))
			{
				field = newValue;
				Draw();
			}
		}

		public double Rating
		{
			get => (double)GetValue(RatingProperty);
			set => SetValue(RatingProperty, value);
		}

		int _max = 5;
		public int Max
		{
			get => _max;
			set => Set(ref _max, value);
		}


		Brush _fillColor = Brush.Yellow;
		public Brush FillColor
		{
			get => _fillColor;
			set => Set(ref _fillColor, value);
		}

		Brush _strokeColor = Brush.Black;
		public Brush StrokeColor
		{
			get => _strokeColor;
			set => Set(ref _strokeColor, value);
		}

		double _strokeThickness = 0;
		public double StrokeThickness
		{
			get => _strokeThickness;
			set => Set(ref _strokeThickness, value);
		}

		double _size = 50;
		public double Size
		{
			get => _size;
			set => Set(ref _size, value);
		}

		private void UpdateTapGestureRecognizers()
		{
			foreach (var star in Children)
			{
				if (!star.GestureRecognizers.Any())
				{
					var recognizer = new TapGestureRecognizer();
					recognizer.Tapped += TapGestureRecognizer_Tapped;
					star.GestureRecognizers.Add(recognizer);
				}
			}
		}

		private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			var star = (View)sender;

			var index = Children.IndexOf(star);

			Rating = index + 1;
		}
	}
}
