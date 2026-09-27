using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Foldable;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class TwoPaneViewLayoutGuideTests
	{
		[Fact]
		public void UsesViewScopedVerticalDivisionRegion()
		{
			var layout = new Grid();
			var service = new TestFoldableService
			{
				Hinge = new Rect(490, 0, 20, 1000),
				Location = Point.Zero,
			};
			var guide = new TwoPaneViewLayoutGuide(layout, service);

			guide.UpdateLayouts(1000, 1000);

			Assert.Same(layout, service.LastHingeView);
			Assert.Equal(TwoPaneViewMode.Wide, guide.Mode);
			Assert.Equal(new Rect(0, 0, 490, 1000), guide.Pane1);
			Assert.Equal(new Rect(510, 0, 490, 1000), guide.Pane2);
		}

		[Fact]
		public void UsesViewScopedHorizontalDivisionRegion()
		{
			var layout = new Grid();
			var service = new TestFoldableService
			{
				Hinge = new Rect(0, 490, 1000, 20),
				Location = Point.Zero,
				Landscape = true,
			};
			var guide = new TwoPaneViewLayoutGuide(layout, service);

			guide.UpdateLayouts(1000, 1000);

			Assert.Equal(TwoPaneViewMode.Tall, guide.Mode);
			Assert.Equal(new Rect(0, 0, 1000, 490), guide.Pane1);
			Assert.Equal(new Rect(0, 510, 1000, 490), guide.Pane2);
		}

		[Fact]
		public void ConvertsWindowDivisionRegionToViewLayout()
		{
			var layout = new Grid();
			var service = new TestFoldableService
			{
				Hinge = new Rect(490, 0, 20, 1000),
				Location = new Point(400, 0),
			};
			var guide = new TwoPaneViewLayoutGuide(layout, service);

			guide.UpdateLayouts(200, 1000);

			Assert.Equal(TwoPaneViewMode.Wide, guide.Mode);
			Assert.Equal(new Rect(0, 0, 90, 1000), guide.Pane1);
			Assert.Equal(new Rect(110, 0, 90, 1000), guide.Pane2);
		}

		[Fact]
		public void NoDivisionRegionPreservesSinglePaneLayout()
		{
			var layout = new Grid();
			var service = new TestFoldableService
			{
				Location = Point.Zero,
			};
			var guide = new TwoPaneViewLayoutGuide(layout, service);

			guide.UpdateLayouts(600, 800);

			Assert.Equal(TwoPaneViewMode.SinglePane, guide.Mode);
			Assert.Equal(new Rect(0, 0, 600, 800), guide.Pane1);
			Assert.Equal(Rect.Zero, guide.Pane2);
		}

		[Fact]
		public void ResponsiveThresholdsSwitchBetweenWideAndSinglePane()
		{
			var view = CreateTwoPaneView(new TestFoldableService());
			view.MinTallModeHeight = double.MaxValue;
			view.MinWideModeWidth = 100;

			view.Measure(300, 300);
			view.Arrange(new Rect(0, 0, 300, 300));

			Assert.Equal(TwoPaneViewMode.Wide, view.Mode);
			Assert.True(((VisualElement)view.Children[0]).IsVisible);
			Assert.True(((VisualElement)view.Children[1]).IsVisible);

			view.MinWideModeWidth = 400;

			Assert.Equal(TwoPaneViewMode.SinglePane, view.Mode);
			Assert.True(((VisualElement)view.Children[0]).IsVisible);
			Assert.False(((VisualElement)view.Children[1]).IsVisible);
		}

		[Fact]
		public void ActiveDivisionOverridesThresholdsAndPaneLengths()
		{
			var service = new TestFoldableService
			{
				Hinge = new Rect(490, 0, 20, 1000),
				Location = Point.Zero,
			};
			var view = CreateTwoPaneView(service);
			view.MinTallModeHeight = double.MaxValue;
			view.MinWideModeWidth = double.MaxValue;
			view.Pane1Length = new GridLength(1, GridUnitType.Star);
			view.Pane2Length = new GridLength(3, GridUnitType.Star);

			view.Measure(1000, 1000);
			view.Arrange(new Rect(0, 0, 1000, 1000));

			Assert.Equal(TwoPaneViewMode.Wide, view.Mode);
			Assert.Equal(490, view.ColumnDefinitions[0].Width.Value);
			Assert.Equal(20, view.ColumnDefinitions[1].Width.Value);
			Assert.Equal(490, view.ColumnDefinitions[2].Width.Value);
		}

		[Fact]
		public void SinglePaneConfigurationHonorsPanePriority()
		{
			var view = CreateTwoPaneView(new TestFoldableService());
			view.MinTallModeHeight = double.MaxValue;
			view.MinWideModeWidth = 0;
			view.WideModeConfiguration = TwoPaneViewWideModeConfiguration.SinglePane;

			view.Measure(300, 300);
			view.Arrange(new Rect(0, 0, 300, 300));

			Assert.True(((VisualElement)view.Children[0]).IsVisible);
			Assert.False(((VisualElement)view.Children[1]).IsVisible);

			view.PanePriority = TwoPaneViewPriority.Pane2;

			Assert.False(((VisualElement)view.Children[0]).IsVisible);
			Assert.True(((VisualElement)view.Children[1]).IsVisible);
		}

		[Fact]
		public void ResponsivePaneLengthsAreApplied()
		{
			var view = CreateTwoPaneView(new TestFoldableService());
			view.MinTallModeHeight = double.MaxValue;
			view.MinWideModeWidth = 0;
			view.Pane1Length = new GridLength(1, GridUnitType.Star);
			view.Pane2Length = new GridLength(3, GridUnitType.Star);

			view.Measure(300, 300);
			view.Arrange(new Rect(0, 0, 300, 300));

			Assert.Equal(TwoPaneViewMode.Wide, view.Mode);
			Assert.Equal(new GridLength(1, GridUnitType.Star), view.ColumnDefinitions[0].Width);
			Assert.Equal(new GridLength(3, GridUnitType.Star), view.ColumnDefinitions[2].Width);
		}

		[Fact]
		public void HorizontalDivisionHonorsBottomTopConfiguration()
		{
			var service = new TestFoldableService
			{
				Hinge = new Rect(0, 490, 1000, 20),
				Location = Point.Zero,
				Landscape = true,
			};
			var view = CreateTwoPaneView(service);
			view.MinTallModeHeight = double.MaxValue;
			view.MinWideModeWidth = double.MaxValue;
			view.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;

			view.Measure(1000, 1000);
			view.Arrange(new Rect(0, 0, 1000, 1000));

			Assert.Equal(TwoPaneViewMode.Tall, view.Mode);
			Assert.Equal(490, view.RowDefinitions[0].Height.Value);
			Assert.Equal(20, view.RowDefinitions[1].Height.Value);
			Assert.Equal(490, view.RowDefinitions[2].Height.Value);
			Assert.Equal(2, view.GetRow(view.Children[0]));
			Assert.Equal(0, view.GetRow(view.Children[1]));
		}

		static TwoPaneView CreateTwoPaneView(TestFoldableService service)
		{
			var pane1 = new BoxView { IsPlatformEnabled = true };
			var pane2 = new BoxView { IsPlatformEnabled = true };
			var view = new TwoPaneView(service)
			{
				IsPlatformEnabled = true,
				Pane1 = pane1,
				Pane2 = pane2,
			};

			((VisualElement)view.Children[0]).IsPlatformEnabled = true;
			((VisualElement)view.Children[1]).IsPlatformEnabled = true;
			return view;
		}

		sealed class TestFoldableService : IFoldableService
		{
			public event EventHandler OnScreenChanged
			{
				add { }
				remove { }
			}

			public event EventHandler<FoldableHingeAngleChangedEventArgs> HingeAngleChanged
			{
				add { }
				remove { }
			}

			public event EventHandler<FoldEventArgs> OnLayoutChanged
			{
				add { }
				remove { }
			}

			public bool IsSpanned => Hinge != Rect.Zero;
			public bool IsLandscape => Landscape;
			public bool Landscape { get; set; }
			public Rect Hinge { get; set; }
			public Point Location { get; set; }
			public VisualElement LastHingeView { get; private set; }
			public Size ScaledScreenSize => new Size(1000, 1000);

			public Rect GetHinge() => Hinge;

			public Rect GetHinge(VisualElement visualElement)
			{
				LastHingeView = visualElement;
				return Hinge;
			}

			public bool IsLandscapeFor(VisualElement visualElement) => Landscape;

			public Size GetScaledScreenSize(VisualElement visualElement) => ScaledScreenSize;

			public Point? GetLocationOnScreen(VisualElement visualElement) => Location;

			public Task<int> GetHingeAngleAsync() => Task.FromResult(0);

			public void StartMonitoring(VisualElement visualElement)
			{
			}

			public void StopMonitoring(VisualElement visualElement)
			{
			}
		}
	}
}
