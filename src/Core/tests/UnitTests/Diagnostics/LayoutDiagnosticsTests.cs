using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Diagnostics
{
	[Category(TestCategory.Core)]
	public class LayoutDiagnosticsTests
	{
		const int AllocationIterations = 1024;
		const string MeasureDurationInstrumentName = "maui.layout.measure_duration";
		const string ArrangeDurationInstrumentName = "maui.layout.arrange_duration";

		static readonly Rect ArrangeBounds = new Rect(0, 0, 100, 100);

		[Fact]
		public void MeasureAndArrangeDoNotAllocateWithoutListeners()
		{
			using var app = CreateMauiApp();
			var view = CreateProbeView(new MauiContext(app.Services));
			var iView = (IView)view;

			for (var i = 0; i < 64; i++)
			{
				iView.Measure(100, 100);
				iView.Arrange(ArrangeBounds);
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
			for (var i = 0; i < AllocationIterations; i++)
			{
				iView.Measure(100, 100);
				iView.Arrange(ArrangeBounds);
			}
			var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

			Assert.Equal(0, allocated);
		}

		[Theory]
		[InlineData(MeasureDurationInstrumentName)]
		[InlineData(ArrangeDurationInstrumentName)]
		public void DurationMetricsRecordWithoutActivityListeners(string instrumentName)
		{
			var measurements = new List<int>();

			using var listener = new MeterListener();
			listener.InstrumentPublished = (instrument, meterListener) =>
			{
				if (instrument.Meter.Name == "Microsoft.Maui" && instrument.Name == instrumentName)
				{
					meterListener.EnableMeasurementEvents(instrument);
				}
			};
			listener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
			{
				if (instrument.Meter.Name == "Microsoft.Maui" && instrument.Name == instrumentName)
				{
					measurements.Add(measurement);
				}
			});
			listener.Start();

			using var app = CreateMauiApp();
			var view = CreateProbeView(new MauiContext(app.Services));
			var iView = (IView)view;

			if (instrumentName == MeasureDurationInstrumentName)
			{
				iView.Measure(100, 100);
			}
			else
			{
				iView.Arrange(ArrangeBounds);
			}

			var measurement = Assert.Single(measurements);
			Assert.True(measurement >= 0);
		}

		[Fact]
		public void ElapsedNanosecondsClampsToIntMaxValue()
		{
			var startTimestamp = Stopwatch.GetTimestamp() - (Stopwatch.Frequency * 3);

			var elapsedNanoseconds = LayoutDiagnosticMetrics.GetElapsedNanoseconds(startTimestamp);

			Assert.Equal(int.MaxValue, elapsedNanoseconds);
		}

		[Fact]
		public void ElapsedNanosecondsReturnsZeroForNonPositiveElapsedTime()
		{
			var startTimestamp = Stopwatch.GetTimestamp() + Stopwatch.Frequency;

			var elapsedNanoseconds = LayoutDiagnosticMetrics.GetElapsedNanoseconds(startTimestamp);

			Assert.Equal(0, elapsedNanoseconds);
		}

		static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IMeterFactory, TestMeterFactory>();

			return builder.Build();
		}

		static ProbeView CreateProbeView(IMauiContext context)
		{
			var view = new ProbeView
			{
				Handler = new ProbeHandler(context),
			};

			return view;
		}

		sealed class ProbeView : View
		{
			protected override Size MeasureOverride(double widthConstraint, double heightConstraint) =>
				new Size(42, 24);

			protected override Size ArrangeOverride(Rect bounds) =>
				bounds.Size;
		}

		sealed class ProbeHandler : IViewHandler
		{
			public ProbeHandler(IMauiContext context)
			{
				MauiContext = context;
			}

			public bool HasContainer { get; set; }

			public object ContainerView => null;

			public object PlatformView => null;

			public IView VirtualView { get; private set; }

			IElement IElementHandler.VirtualView => VirtualView;

			public IMauiContext MauiContext { get; private set; }

			public void SetMauiContext(IMauiContext mauiContext) =>
				MauiContext = mauiContext;

			public void SetVirtualView(IElement view) =>
				VirtualView = (IView)view;

			public void UpdateValue(string property)
			{
			}

			public void Invoke(string command, object args = null)
			{
			}

			public void DisconnectHandler()
			{
			}

			public Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
				new Size(42, 24);

			public void PlatformArrange(Rect frame)
			{
			}
		}

		sealed class TestMeterFactory : IMeterFactory
		{
			readonly List<Meter> _meters = new List<Meter>();

			public Meter Create(MeterOptions options)
			{
				var meter = new Meter(options);
				_meters.Add(meter);

				return meter;
			}

			public void Dispose()
			{
				foreach (var meter in _meters)
				{
					meter.Dispose();
				}
			}
		}
	}
}
