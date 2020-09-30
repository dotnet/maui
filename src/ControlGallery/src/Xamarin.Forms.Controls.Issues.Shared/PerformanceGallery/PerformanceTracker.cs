using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	public class PerformanceTracker : ContentView
	{
		public const string RenderCompleteMessage = "RenderComplete";
		public PerformanceTrackerWatcher Watcher { get; private set; }

		public static readonly BindableProperty RenderTimeProperty = BindableProperty.Create(nameof(RenderTime), typeof(float), typeof(PerformanceTracker), 0f);
		public float RenderTime
		{
			get { return (float)GetValue(RenderTimeProperty); }
			set { SetValue(RenderTimeProperty, value); }
		}

		public static readonly BindableProperty ScenarioProperty = BindableProperty.Create(nameof(Scenario), typeof(string), typeof(PerformanceTracker), "Generating report...");
		public string Scenario
		{
			get { return (string)GetValue(ScenarioProperty); }
			set { SetValue(ScenarioProperty, value); }
		}

		public static readonly BindableProperty ExpectedRenderTimeProperty = BindableProperty.Create(nameof(ExpectedRenderTime), typeof(float), typeof(PerformanceTracker), 0f);
		public float ExpectedRenderTime
		{
			get { return (float)GetValue(ExpectedRenderTimeProperty); }
			set { SetValue(ExpectedRenderTimeProperty, value); }
		}

		public static readonly BindableProperty OutcomeProperty = BindableProperty.Create(nameof(Outcome), typeof(string), typeof(PerformanceTracker), "Generating report...");
		public string Outcome
		{
			get { return (string)GetValue(OutcomeProperty); }
			set { SetValue(OutcomeProperty, value); }
		}
		public long TotalMilliseconds { get; set; }

		public PerformanceTracker()
		{
			Watcher = new PerformanceTrackerWatcher(this);
			ControlTemplate = new ControlTemplate(typeof(PerformanceTrackerTemplate));
			SetBinding(RenderTimeProperty, new Binding(nameof(PerformanceViewModel.ActualRenderTime)));
			SetBinding(ContentProperty, new Binding(nameof(PerformanceViewModel.View)));
			SetBinding(ScenarioProperty, new Binding(nameof(PerformanceViewModel.Scenario)));
			SetBinding(ExpectedRenderTimeProperty, new Binding(nameof(PerformanceViewModel.ExpectedRenderTime)));
			SetBinding(OutcomeProperty, new Binding(nameof(PerformanceViewModel.Outcome)));
		}
	}
}
