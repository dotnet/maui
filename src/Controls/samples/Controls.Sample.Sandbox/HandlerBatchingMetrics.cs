using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample;

static class HandlerBatchingMetrics
{
	static long _mapperCalls;
	static long _measureInvalidations;
	static readonly ConcurrentDictionary<string, long> MapperCallsByKey = new(StringComparer.Ordinal);

#if EXPERIMENTAL_HANDLER_UPDATE_BATCHING
	public const string Mode = "batched";
#else
	public const string Mode = "stock";
#endif

	public static long MapperCalls => Interlocked.Read(ref _mapperCalls);

	public static long MeasureInvalidations => Interlocked.Read(ref _measureInvalidations);

	public static string MapperCallSummary =>
		string.Join(",", MapperCallsByKey
			.OrderBy(pair => pair.Key, StringComparer.Ordinal)
			.Select(pair => $"{pair.Key}:{pair.Value}"));

	public static void Configure()
	{
		ViewHandler.ViewMapper.AppendToMapping(nameof(IView.Opacity), Record(nameof(IView.Opacity)));
		ViewHandler.ViewCommandMapper.AppendToMapping(nameof(IView.InvalidateMeasure), RecordMeasureInvalidation);

		LabelHandler.Mapper.AppendToMapping(nameof(ITextStyle.TextColor), Record(nameof(ITextStyle.TextColor)));

		ButtonHandler.Mapper.AppendToMapping(nameof(ITextStyle.TextColor), Record(nameof(ITextStyle.TextColor)));

		EntryHandler.Mapper.AppendToMapping(nameof(IEntry.Placeholder), Record(nameof(IEntry.Placeholder)));
		EntryHandler.Mapper.AppendToMapping(nameof(IEntry.TextColor), Record(nameof(IEntry.TextColor)));

		SwitchHandler.Mapper.AppendToMapping(nameof(ISwitch.IsOn), Record(nameof(ISwitch.IsOn)));
		SwitchHandler.Mapper.AppendToMapping(nameof(ISwitch.ThumbColor), Record(nameof(ISwitch.ThumbColor)));

		SliderHandler.Mapper.AppendToMapping(nameof(ISlider.Value), Record(nameof(ISlider.Value)));
		SliderHandler.Mapper.AppendToMapping(nameof(ISlider.MinimumTrackColor), Record(nameof(ISlider.MinimumTrackColor)));
		SliderHandler.Mapper.AppendToMapping(nameof(ISlider.MaximumTrackColor), Record(nameof(ISlider.MaximumTrackColor)));

		ProgressBarHandler.Mapper.AppendToMapping(nameof(IProgress.Progress), Record(nameof(IProgress.Progress)));
		ProgressBarHandler.Mapper.AppendToMapping(nameof(IProgress.ProgressColor), Record(nameof(IProgress.ProgressColor)));
	}

	public static void Reset()
	{
		Interlocked.Exchange(ref _mapperCalls, 0);
		Interlocked.Exchange(ref _measureInvalidations, 0);
		MapperCallsByKey.Clear();
	}

	static Action<IElementHandler, IElement> Record(string property) =>
		(handler, view) =>
		{
			Interlocked.Increment(ref _mapperCalls);
			MapperCallsByKey.AddOrUpdate($"{view.GetType().Name}.{property}", 1, static (_, count) => count + 1);
		};

	static void RecordMeasureInvalidation(IViewHandler handler, IView view, object? args) =>
		Interlocked.Increment(ref _measureInvalidations);
}
