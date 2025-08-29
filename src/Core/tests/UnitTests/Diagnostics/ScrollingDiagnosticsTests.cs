using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Maui.Diagnostics;
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
	}
}