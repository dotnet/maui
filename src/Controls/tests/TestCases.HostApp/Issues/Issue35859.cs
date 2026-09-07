using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35859, "CollectionView2 on iOS measures non-first cells despite ItemSizingStrategy.MeasureFirstItem", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35859 : ContentPage
{
	readonly Label _summaryLabel;
	readonly CollectionView2 _collectionView;

	public Issue35859()
	{
		Title = "Issue 35859";

		MeasureFirstItemProbeRegistry.Reset();
		MeasureFirstItemProbeRegistry.MeasurementsChanged += OnMeasurementsChanged;

		_summaryLabel = new Label
		{
			AutomationId = "35859Summary",
			Margin = new Thickness(12, 10),
			FontSize = 13
		};

		var resetButton = new Button
		{
			Text = "Reset",
			AutomationId = "35859ResetButton"
		};
		resetButton.Clicked += (_, _) => ResetProof();

		var scrollButton = new Button
		{
			Text = "Scroll to 40",
			AutomationId = "35859ScrollTo40Button"
		};
		scrollButton.Clicked += (_, _) => ScrollToItem40();

		var buttons = new HorizontalStackLayout
		{
			Spacing = 8,
			Margin = new Thickness(12, 0, 12, 10),
			Children = { resetButton, scrollButton }
		};

		_collectionView = CreateCollectionView();

		var headerLabel = new Label
		{
			Text = "MeasureFirstItem regression probe for CV2",
			Margin = new Thickness(12, 10, 12, 0),
			FontAttributes = FontAttributes.Bold
		};

		var layout = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		layout.Children.Add(headerLabel);
		Grid.SetRow(headerLabel, 0);

		layout.Children.Add(_summaryLabel);
		Grid.SetRow(_summaryLabel, 1);

		layout.Children.Add(buttons);
		Grid.SetRow(buttons, 2);

		layout.Children.Add(_collectionView);
		Grid.SetRow(_collectionView, 3);

		Content = layout;

		UpdateSummary();
	}

	static CollectionView2 CreateCollectionView()
	{
		return new CollectionView2
		{
			AutomationId = "35859Items2CV2CollectionView",
			ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 4 },
			ItemTemplate = new DataTemplate(() => new MeasureFirstItemProbeCell()),
			ItemsSource = CreateItems()
		};
	}

	static List<MeasureFirstItemProbeItem> CreateItems()
	{
		var items = new List<MeasureFirstItemProbeItem>();
		for (int index = 0; index < 80; index++)
		{
			items.Add(new MeasureFirstItemProbeItem(index));
		}

		return items;
	}

	void ResetProof()
	{
		MeasureFirstItemProbeRegistry.Reset();

		_collectionView.ItemsSource = null;
		_collectionView.ItemsSource = CreateItems();
		_collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);

		UpdateSummary();
	}

	void ScrollToItem40()
	{
		_collectionView.ScrollTo(40, position: ScrollToPosition.Start, animate: false);
	}

	protected override void OnDisappearing()
	{
		MeasureFirstItemProbeRegistry.MeasurementsChanged -= OnMeasurementsChanged;
		base.OnDisappearing();
	}

	void OnMeasurementsChanged()
	{
		Dispatcher.Dispatch(UpdateSummary);
	}

	void UpdateSummary()
	{
		_summaryLabel.Text = MeasureFirstItemProbeRegistry.GetSummary();
	}
}

public sealed class MeasureFirstItemProbeItem
{
	public MeasureFirstItemProbeItem(int index)
	{
		Index = index;
		Title = $"Item {Index}";
	}

	public int Index { get; }

	public string Title { get; }
}

static class MeasureFirstItemProbeRegistry
{
	static readonly object Lock = new();
	static readonly Dictionary<int, MeasureFirstItemProbeRecord> Records = new();

	public static event Action MeasurementsChanged;

	public static void Reset()
	{
		lock (Lock)
		{
			Records.Clear();
		}

		MeasurementsChanged?.Invoke();
	}

	public static void RecordMeasurement(MeasureFirstItemProbeItem item, double heightConstraint, double measuredHeight)
	{
		lock (Lock)
		{
			if (!Records.TryGetValue(item.Index, out var record))
			{
				record = new MeasureFirstItemProbeRecord(item);
				Records[item.Index] = record;
			}

			record.MeasureCount++;
			record.HeightConstraints.Add(heightConstraint);

			if (item.Index == 0 && record.MeasuredHeight <= 0 && measuredHeight > 0)
			{
				record.MeasuredHeight = measuredHeight;
			}
		}

		MeasurementsChanged?.Invoke();
	}

	public static string GetSummary()
	{
		lock (Lock)
		{
			var firstMeasuredHeight = GetFirstMeasuredHeight();
			var cachedHeightMeasuredNonFirst = 0;

			foreach (var record in Records.Values)
			{
				if (record.Index == 0 || record.MeasureCount == 0)
				{
					continue;
				}

				if (HasCachedHeightMeasure(record, firstMeasuredHeight))
				{
					cachedHeightMeasuredNonFirst++;
				}
			}

			return $"Items2 CV2: {cachedHeightMeasuredNonFirst} cached-height non-first";
		}
	}

	static double GetFirstMeasuredHeight()
	{
		foreach (var record in Records.Values)
		{
			if (record.Index == 0 && record.MeasuredHeight > 0)
			{
				return record.MeasuredHeight;
			}
		}

		return 0;
	}

	static bool HasCachedHeightMeasure(MeasureFirstItemProbeRecord record, double firstHeight)
	{
		if (firstHeight <= 0)
		{
			return false;
		}

		foreach (var heightConstraint in record.HeightConstraints)
		{
			if (!double.IsInfinity(heightConstraint) && Math.Abs(heightConstraint - firstHeight) < 0.5)
			{
				return true;
			}
		}

		return false;
	}

	sealed class MeasureFirstItemProbeRecord
	{
		public MeasureFirstItemProbeRecord(MeasureFirstItemProbeItem item)
		{
			Item = item;
		}

		public MeasureFirstItemProbeItem Item { get; }

		public int Index => Item.Index;

		public int MeasureCount { get; set; }

		public double MeasuredHeight { get; set; }

		public List<double> HeightConstraints { get; } = new();
	}
}

sealed class MeasureFirstItemProbeCell : Grid
{
	MeasureFirstItemProbeItem _item;

	public MeasureFirstItemProbeCell()
	{
		Padding = new Thickness(12, 8);
		var titleLabel = new Label
		{
			VerticalOptions = LayoutOptions.Center
		};
		titleLabel.SetBinding(Label.TextProperty, nameof(MeasureFirstItemProbeItem.Title));
		Children.Add(titleLabel);
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();
		_item = BindingContext as MeasureFirstItemProbeItem;
	}

	protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
	{
		var size = base.MeasureOverride(widthConstraint, heightConstraint);

		if (_item is not null)
		{
			MeasureFirstItemProbeRegistry.RecordMeasurement(_item, heightConstraint, size.Height);
		}

		return size;
	}
}
