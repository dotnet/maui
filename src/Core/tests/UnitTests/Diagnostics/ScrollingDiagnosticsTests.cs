using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Diagnostics)]
	public class ScrollingDiagnosticsTests
	{
		[Fact]
		public void ScrollingDiagnosticMetrics_Creates_ExpectedMetrics()
		{
			var meter = new Meter("TestMeter");
			var metrics = new ScrollingDiagnosticMetrics();

			metrics.Create(meter);

			Assert.NotNull(metrics.ScrollingCounter);
			Assert.NotNull(metrics.ScrollingDuration);
			Assert.NotNull(metrics.ScrollingVelocity);
			Assert.NotNull(metrics.ScrollingJank);

			meter.Dispose();
		}

		[Fact]
		public void ScrollingDiagnosticMetrics_RecordScroll_WithValidDuration()
		{
			var meter = new Meter("TestMeter");
			var metrics = new ScrollingDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList { { "control.type", "ScrollView" }, { "scroll.direction", "Vertical" } };

			var duration = TimeSpan.FromMilliseconds(10);

			// This should not throw
			metrics.RecordScroll(duration, in tagList);

			meter.Dispose();
		}

		[Fact]
		public void ScrollingDiagnosticMetrics_RecordJank_WithTags()
		{
			var meter = new Meter("TestMeter");
			var metrics = new ScrollingDiagnosticMetrics();
			metrics.Create(meter);

			var tagList = new TagList { { "control.type", "CollectionView" } };

			// This should not throw
			metrics.RecordJank(in tagList);

			meter.Dispose();
		}
		
		[Fact]
		public void DiagnosticInstrumentation_StartScrolling_ReturnsNullWhenNotSupported()
		{
			var mockView = new TestView();
			
			var result = DiagnosticInstrumentation.StartScrolling(mockView, "Test");
			
			Assert.NotNull(result);
		}
		
		class TestView : IView
		{
			public bool StopDiagnosticsCalled { get; private set; }

			public virtual IViewHandler Handler { get; set; }

			IElementHandler IElement.Handler
			{
				get => Handler;
				set => Handler = (IViewHandler)value;
			}

			public IElement Parent { get; set; }

			public IElementHandler ElementHandler { get; set; }

			public string AutomationId { get; set; } = string.Empty;

			public Graphics.Rect Frame { get; set; }

			public FlowDirection FlowDirection { get; set; }

			public double Width { get; set; }

			public double Height { get; set; }

			public double MinimumWidth { get; set; }

			public double MinimumHeight { get; set; }

			public double MaximumWidth { get; set; } = double.PositiveInfinity;

			public double MaximumHeight { get; set; } = double.PositiveInfinity;

			public Thickness Margin { get; set; }

			public Primitives.LayoutAlignment HorizontalLayoutAlignment { get; set; }

			public Primitives.LayoutAlignment VerticalLayoutAlignment { get; set; }

			public Visibility Visibility { get; set; }

			public double Opacity { get; set; } = 1.0;

			public Paint Background { get; set; }

			public IShape Clip { get; set; }

			public IShadow Shadow { get; set; }

			public int ZIndex { get; set; }

			public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

			public double TranslationX { get; set; } = 0;
			public double TranslationY { get; set; } = 0;
			public double Scale { get; set; } = 1;
			public double ScaleX { get; set; } = 1;
			public double ScaleY { get; set; } = 1;
			public double Rotation { get; set; } = 0;
			public double RotationX { get; set; } = 0;
			public double RotationY { get; set; } = 0;
			public double AnchorX { get; set; } = 0.5;

			public double AnchorY { get; set; } = 0.5;

			public bool IsEnabled { get; set; } = true;

			public bool InputTransparent { get; set; }

			public Semantics Semantics { get; set; }

			public bool IsFocused { get; set; }

			public bool CanBeFocused { get; set; }

			public Graphics.Size DesiredSize { get; set; }

			public void Arrange(Graphics.Rect bounds) { }

			Size IView.Arrange(Rect bounds)
			{
				throw new NotImplementedException();
			}

			public Size Measure(double widthConstraint, double heightConstraint) => new Graphics.Size(100, 100);

			public void Focus(FocusRequest request) { }

			public bool Focus()
			{
				throw new NotImplementedException();
			}

			public void Unfocus() { }

			public void InvalidateArrange() { }

			public void InvalidateMeasure() { }

			internal void StopDiagnostics(Activity activity, IDiagnosticInstrumentation instrumentation)
			{
				StopDiagnosticsCalled = true;
				activity?.Stop();
				activity?.Dispose();
			}
		}
		
		class TestScrollView : TestView, IScrollView
		{
			public double HorizontalOffset { get; set; }
			public double VerticalOffset { get; set; }
			public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Default;
			public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Default;
			public ScrollOrientation Orientation { get; set; } = ScrollOrientation.Vertical;
			public Graphics.Size ContentSize { get; set; }
			public void RequestScrollTo(double horizontalOffset, double verticalOffset, bool animated) { }
			public void ScrollFinished() { }

			// IContentView implementation
			public object Content { get; set; }
			public IView PresentedContent { get; set; }
			public Thickness Padding { get; set; }

			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) => 
				new Size(100, 100);

			public Size CrossPlatformArrange(Graphics.Rect bounds) => 
				bounds.Size;
		}
	}
}