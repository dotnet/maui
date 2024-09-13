using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
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

	public long? GetArrangeTicks() {
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

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.None, 0, "Rendering performance", PlatformAffected.All)]
public partial class RenderingPerformance : ContentPage
{
	bool _firstRun = true;

	public List<Model> Models { get; set; }

	public RenderingPerformance()
	{
		Models = GenerateMeasuredItem();
		BindingContext = this;
		InitializeComponent();
	}

	private async void ButtonClicked(object sender, EventArgs e)
	{
		var capturedTimes = new List<int[]>();

		var test1Models = GenerateItems(50, "Test1");
		var test2Models = GenerateItems(25, "Test2");
		var resetModel = GenerateMeasuredItem();

		if (_firstRun)
		{
			_firstRun = false;
			await GetArrangeTicksAsync();
		}

		for (var i = 0; i < 6; i++)
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

	async Task<long> GetArrangeTicksAsync()
	{
		while (true)
		{
			await Task.Delay(100);
			IView view = BindableContainer;
			while (view is ILayout { Count: > 0 } layout)
			{
				view = layout[^1];
			}

			if (view is MeasuredLabel measuredLabel && measuredLabel.GetArrangeTicks() is { } arrangeTicks)
			{
				measuredLabel.LastArrangedTicks = null;
				return arrangeTicks;
			}
		}
	}

	static List<Model> GenerateItems(int count, string prefix)
	{
		return
		[
			..Enumerable.Range(0, count).Select(i => new Model
			{
				Content = $"{prefix} Content {i}",
				Header = $"Header {i}",
				SubModels = Enumerable.Range(0, 10).Select(j => new SubModel
				{
					Content = $"{prefix} SubContent {j}", Header = $"{prefix} SubHeader {j}"
				}).ToArray()
			}),
			..GenerateMeasuredItem()
		];
	}

	static List<Model> GenerateMeasuredItem()
	{
		return
		[
			new Model
			{
				Content = "Measured",
				Header = "Measured",
				SubModels =
				[
					new SubModel { Content = "Measured", Header = "Measured", IsMeasured = true }
				]
			}
		];
	}

	public class Model : SubModel
	{
		public SubModel[] SubModels { get; set; }
	}

	public class SubModel
	{
		public string Header { get; set; }
		public string Content { get; set; }
		public bool IsMeasured { get; set; }
	}
}