using System.Diagnostics;
using ILayout = Microsoft.Maui.ILayout;

namespace Maui.Controls.Sample.Issues;

public class MeasuredLabel : Label
{
	private static readonly TimeSpan ArrangedThreshold = TimeSpan.FromSeconds(1);
	public static readonly BindableProperty IsMeasuredProperty = BindableProperty.Create(nameof(IsMeasured), typeof(bool), typeof(MeasuredLabel), false);

	public bool IsMeasured
	{
		get => (bool)GetValue(IsMeasuredProperty);
		set => SetValue(IsMeasuredProperty, value);
	}

	public long? LastArrangedTicks { get; set; }

	public long? GetArrangeTicks()
	{
		if (LastArrangedTicks is { } ticks)
		{
			var elapsed = Stopwatch.GetElapsedTime(ticks);
			if (elapsed > ArrangedThreshold)
			{
				return ticks;
			}
		}

		return null;
	}
}

public static class RenderingPerformanceExtensions
{
	public static MauiAppBuilder RenderingPerformanceAddMappers(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(handlers =>
		{
			Microsoft.Maui.Handlers.LabelHandler.CommandMapper.AppendToMapping(nameof(IView.Frame), (handler, view, arg) =>
			{
				if (view is MeasuredLabel { IsMeasured: true } measuredLabel)
				{
					measuredLabel.LastArrangedTicks = Stopwatch.GetTimestamp();
				}
			});
		});

		return builder;
	}
}

[Issue(IssueTracker.None, 0, "Rendering performance", PlatformAffected.All)]
public partial class RenderingPerformance : ContentPage
{
	bool _firstRun = true;

	public List<ViewModelStub> Models { get; set; }

	public RenderingPerformance()
	{
		Models = GenerateMeasuredItem();
		BindingContext = this;
		InitializeComponent();
	}

	private async void ButtonClicked(object sender, EventArgs e)
	{
		var capturedTimes = new List<int[]>();

		// Generate view models so that only the last NestedViewModelStub of the last ViewModelStub is measured 
		// First time we generate 40 * 10 + 1 = 401 items
		// This causes the creation of (40 * 5) + (40 * 10 * 4) + (1 * 5) + (1 * 4) = ~1800 platform views
		var test1Models = GenerateItems(40, "First");
		// Second time we generate 20 * 10 + 1 = 201 items
		// This causes (20 * 5) + (20 * 10 * 4) = ~900 binding context changes
		// and other ~900 platform views removals
		var test2Models = GenerateItems(20, "Second");
		// Third time we manually clear the BindableContainer and reset the models to the initial state (1 measured item) 
		var resetModel = GenerateMeasuredItem();

		// This enables us to measure the time it takes to:
		// - Create platform views
		// - Bind the new view models
		// - Remove platform views
		// - Clear platform views

		// Views include frequently used components like `ContentView` (legacy layout), `Border`, `VerticalStackLayout`, `Grid`, `Label`.
		// Measurement happens by tracking IView.Frame mapping which happens right after the platform has arranged the view in the container view.

		// Clear the first measure (happened while rendering the page for the first time)
		if (_firstRun)
		{
			_firstRun = false;
			await GetArrangeTicksAsync();
		}

		for (var i = 0; i < 5; i++)
		{
			await Task.Delay(200);

			Models = test1Models;
			var startTicks = Stopwatch.GetTimestamp();
			OnPropertyChanged(nameof(Models));
			var endTicks = await Task.Run(GetArrangeTicksAsync);
			var t1 = (int)Stopwatch.GetElapsedTime(startTicks, endTicks).TotalMilliseconds;

			await Task.Delay(200);

			Models = test2Models;
			startTicks = Stopwatch.GetTimestamp();
			OnPropertyChanged(nameof(Models));
			endTicks = await Task.Run(GetArrangeTicksAsync);
			var t2 = (int)Stopwatch.GetElapsedTime(startTicks, endTicks).TotalMilliseconds;

			await Task.Delay(200);

			startTicks = Stopwatch.GetTimestamp();
			BindableContainer.Clear();
			Models = resetModel;
			OnPropertyChanged(nameof(Models));
			endTicks = await Task.Run(GetArrangeTicksAsync);
			var t3 = (int)Stopwatch.GetElapsedTime(startTicks, endTicks).TotalMilliseconds;

			capturedTimes.Add([t1, t2, t3]);
		}

		var avg1 = (int)capturedTimes.Average(t => t[0]);
		var avg2 = (int)capturedTimes.Average(t => t[1]);
		var avg3 = (int)capturedTimes.Average(t => t[2]);
		StartButton.Text = $"{avg1},{avg2},{avg3}";
	}

	/// <summary>
	/// Traverse the visual tree to find the last MeasuredLabel and return its arrange ticks when found
	/// </summary>
	/// <returns></returns>
	async Task<long> GetArrangeTicksAsync()
	{
		while (true)
		{
			await Task.Delay(100);
			IView view = BindableContainer;
			while (true)
			{
				if (view is ILayout { Count: > 0 } layout)
				{
					view = layout[^1];
				}
				else if (view is IContentView contentView)
				{
					view = (IView)contentView.Content;
				}
				else
				{
					break;
				}
			}

			if (view is MeasuredLabel measuredLabel && measuredLabel.GetArrangeTicks() is { } arrangeTicks)
			{
				measuredLabel.LastArrangedTicks = null;
				return arrangeTicks;
			}
		}
	}

	static List<ViewModelStub> GenerateItems(int count, string prefix)
	{
		return
		[
			..Enumerable.Range(0, count).Select(i => new ViewModelStub
			{
				Content = $"{prefix} Content {i}",
				Header = $"Header {i}",
				SubModels = Enumerable.Range(0, 10).Select(j => new NestedViewModelStub
				{
					Content = $"{prefix} SubContent {j}", Header = $"{prefix} SubHeader {j}"
				}).ToArray()
			}),
			..GenerateMeasuredItem()
		];
	}

	static List<ViewModelStub> GenerateMeasuredItem()
	{
		return
		[
			new ViewModelStub
			{
				Content = "Measured Content",
				Header = "Measured Header",
				SubModels =
				[
					new NestedViewModelStub { Content = "Measured SubContent", Header = "Measured SubHeader", IsMeasured = true }
				]
			}
		];
	}

	public class ViewModelStub
	{
		public string Header { get; set; }
		public string Content { get; set; }
		public NestedViewModelStub[] SubModels { get; set; }
	}

	public class NestedViewModelStub
	{
		public string Header { get; set; }
		public string Content { get; set; }
		public bool IsMeasured { get; set; }
	}
}