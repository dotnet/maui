using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Benchmarks
{
	[MemoryDiagnoser]
	public class LayoutDiagnosticsBenchmarker
	{
		const int SingleIterations = 1024;
		const int SingleOperationsPerInvoke = SingleIterations * 2;
		const int TreeChildCount = 100;
		const int TreeIterations = 16;
		const int TreeOperationsPerInvoke = TreeChildCount * TreeIterations * 2;

		static readonly Rect ArrangeBounds = new(0, 0, 100, 100);
		static readonly Rect TreeBounds = new(0, 0, 100, 2400);

		MauiApp _app;
		MauiContext _context;
		IView _view;
		ILayout _layout;

		[GlobalSetup]
		public void Setup()
		{
			var builder = MauiApp.CreateBuilder();
			builder.UseMauiApp<Application>();

			_app = builder.Build();
			_context = new MauiContext(_app.Services);
			_view = CreateProbeView(_context);

			var layout = new VerticalStackLayout
			{
				Spacing = 0,
			};

			for (var i = 0; i < TreeChildCount; i++)
			{
				layout.Add(CreateProbeView(_context));
			}

			_layout = layout;
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_app?.Dispose();
		}

		[Benchmark(OperationsPerInvoke = SingleOperationsPerInvoke)]
		public double MeasureAndArrangeViewNoListener()
		{
			double checksum = 0;

			for (var i = 0; i < SingleIterations; i++)
			{
				var measured = _view.Measure(100, 100);
				var arranged = _view.Arrange(ArrangeBounds);
				checksum += measured.Width + arranged.Height;
			}

			return checksum;
		}

		[Benchmark(OperationsPerInvoke = TreeOperationsPerInvoke)]
		public double MeasureAndArrangeTreeNoListener()
		{
			double checksum = 0;

			for (var i = 0; i < TreeIterations; i++)
			{
				var measured = _layout.CrossPlatformMeasure(100, 2400);
				var arranged = _layout.CrossPlatformArrange(TreeBounds);
				checksum += measured.Width + arranged.Height;
			}

			return checksum;
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
				new(42, 24);

			protected override Size ArrangeOverride(Rect bounds)
			{
				Frame = bounds;
				return bounds.Size;
			}
		}

		sealed class ProbeHandler(IMauiContext context) : IViewHandler
		{
			public bool HasContainer { get; set; }

			public object ContainerView => null;

			public object PlatformView => null;

			public IView VirtualView { get; private set; }

			IElement IElementHandler.VirtualView => VirtualView;

			public IMauiContext MauiContext { get; private set; } = context;

			public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;

			public void SetVirtualView(IElement view)
			{
				VirtualView = (IView)view;
			}

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
				new(42, 24);

			public void PlatformArrange(Rect frame)
			{
			}
		}
	}
}
