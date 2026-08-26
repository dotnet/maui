using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

/// <summary>How the heavy content is presented. "Popup" in the customer report is a third-party
/// PopupPage hosted in the window; the closest first-party equivalents are a modal page and an
/// in-page overlay, so both are measured against a plain navigation push.</summary>
public enum PerfHost
{
	Modal,
	NavigationPush,
	InPageOverlay,
}

/// <summary>Which instrumentation is attached for a given run of a scenario.</summary>
public enum PerfPass
{
	/// <summary>No listeners attached: clean wall-clock timings.</summary>
	Time,

	/// <summary>MeterListener attached: framework measure/arrange call counts.</summary>
	Count,

	/// <summary>MeterListener + ActivityListener: per-element-type measure durations.</summary>
	Duration,

	/// <summary>ActivityListener attached: handler realization phases and mapper-key durations.</summary>
	Attribution,
}

public enum PerfContent
{
	/// <summary>CollectionView + deeply nested item template (mirrors the customer tile).</summary>
	CollectionViewHeavy,

	/// <summary>CollectionView + flattened item template (same text, ~1/4 of the elements).</summary>
	CollectionViewFlat,

	/// <summary>CollectionView + nested template but with an explicit item height.</summary>
	CollectionViewHeavyFixedHeight,

	/// <summary>ScrollView + VerticalStackLayout of nested tiles: no virtualization at all.</summary>
	NonVirtualizedStack,
}

public sealed class PerfScenario
{
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required PerfHost Host { get; init; }
	public required PerfContent Content { get; init; }
	public ItemSizingStrategy Sizing { get; init; } = ItemSizingStrategy.MeasureAllItems;
	public int ItemCount { get; init; } = 280;
	public double FixedItemHeight { get; init; } = 360;

	public override string ToString() => Id;
}

public sealed class PerfContentTree
{
	public required View Root { get; init; }
	public CollectionView? List { get; init; }
}

public static class PerfContentFactory
{
	public const double FixedTileHeight = 360;

	public static PerfContentTree Build(PerfScenario scenario, IReadOnlyList<FeedItem> items)
	{
		switch (scenario.Content)
		{
			case PerfContent.NonVirtualizedStack:
				return BuildStack(scenario, items);

			default:
				return BuildCollectionView(scenario, items);
		}
	}

	static PerfContentTree BuildCollectionView(PerfScenario scenario, IReadOnlyList<FeedItem> items)
	{
		DataTemplate template = scenario.Content switch
		{
			PerfContent.CollectionViewFlat => new DataTemplate(static () => new FlatFeedCardView()),
			PerfContent.CollectionViewHeavyFixedHeight => new DataTemplate(static () => new HeavyFeedCardView { HeightRequest = FixedTileHeight }),
			_ => new DataTemplate(static () => new HeavyFeedCardView()),
		};

		var list = new CollectionView
		{
			AutomationId = "PerfCollectionView",
			ItemsSource = items,
			ItemSizingStrategy = scenario.Sizing,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 },
			Margin = new Thickness(8, 6, 8, 6),
			VerticalOptions = LayoutOptions.Fill,
			ItemTemplate = template,
		};

		return new PerfContentTree { Root = list, List = list };
	}

	static PerfContentTree BuildStack(PerfScenario scenario, IReadOnlyList<FeedItem> items)
	{
		var stack = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(8, 6) };

		int count = Math.Min(scenario.ItemCount, items.Count);
		for (int i = 0; i < count; i++)
		{
			stack.Add(new HeavyFeedCardView { BindingContext = items[i] });
		}

		var scroll = new ScrollView
		{
			AutomationId = "PerfScrollView",
			Content = stack,
			VerticalOptions = LayoutOptions.Fill,
		};

		return new PerfContentTree { Root = scroll };
	}

	/// <summary>Counts the realized visual elements in a tree; used to normalize measure counts per element.</summary>
	public static int CountVisualElements(IVisualTreeElement root)
	{
		int count = 1;
		var children = root.GetVisualChildren();
		for (int i = 0; i < children.Count; i++)
		{
			count += CountVisualElements(children[i]);
		}

		return count;
	}
}
