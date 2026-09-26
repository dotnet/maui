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
