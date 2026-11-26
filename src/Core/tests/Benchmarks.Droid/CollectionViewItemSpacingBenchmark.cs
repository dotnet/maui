using System.Collections.ObjectModel;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace Benchmarks.Droid;

// Measures collection changes on an Android CollectionView with and without ItemSpacing. Each operation changes
// the items, runs a synchronous layout pass, reverts the change and lays out again, so every iteration starts from
// the same state. Everything runs on the main thread because CollectionView dispatches collection changes there.
public class CollectionViewItemSpacingBenchmark
{
	const int ItemCount = 50;
	const int Width = 1080;
	const int Height = 1920;
	const int Operations = 20;

	ObservableCollection<string> _spacedItems = null!;
	ObservableCollection<string> _unspacedItems = null!;
	ObservableCollection<string> _gridItems = null!;
	Android.Views.View _spacedView = null!;
	Android.Views.View _unspacedView = null!;
	Android.Views.View _gridView = null!;

	[GlobalSetup]
	public void GlobalSetup() => RunOnMain(() =>
	{
		var app = MauiApp.CreateBuilder()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CollectionView>(_ => new CollectionViewHandler());
				handlers.AddHandler<Label>(_ => new LabelHandler());
			})
			.Build();
		var context = new MauiContext(app.Services, Android.App.Application.Context);

		(_spacedItems, _spacedView) = Create(context, new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 8 });
		(_unspacedItems, _unspacedView) = Create(context, new LinearItemsLayout(ItemsLayoutOrientation.Vertical));
		(_gridItems, _gridView) = Create(context, new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) { VerticalItemSpacing = 8, HorizontalItemSpacing = 8 });
	});

	[Benchmark(OperationsPerInvoke = Operations)]
	public void AppendWithSpacing() => Append(_spacedItems, _spacedView);

	[Benchmark(OperationsPerInvoke = Operations)]
	public void AppendWithoutSpacing() => Append(_unspacedItems, _unspacedView);

	[Benchmark(OperationsPerInvoke = Operations)]
	public void AppendToGridWithSpacing() => Append(_gridItems, _gridView);

	[Benchmark(OperationsPerInvoke = Operations)]
	public void InsertAtStartWithSpacing() => RunOnMain(() =>
	{
		for (int i = 0; i < Operations; i++)
		{
			_spacedItems.Insert(0, "New");
			LayoutPass(_spacedView);
			_spacedItems.RemoveAt(0);
			LayoutPass(_spacedView);
		}
	});

	[Benchmark(OperationsPerInvoke = Operations)]
	public void ReplaceOffscreenItemWithSpacing() => RunOnMain(() =>
	{
		var last = ItemCount - 1;
		for (int i = 0; i < Operations; i++)
		{
			_spacedItems[last] = "Replaced";
			LayoutPass(_spacedView);
			_spacedItems[last] = $"Item {last}";
			LayoutPass(_spacedView);
		}
	});

	static void Append(ObservableCollection<string> items, Android.Views.View view) => RunOnMain(() =>
	{
		for (int i = 0; i < Operations; i++)
		{
			items.Add("New");
			LayoutPass(view);
			items.RemoveAt(items.Count - 1);
			LayoutPass(view);
		}
	});

	static (ObservableCollection<string>, Android.Views.View) Create(IMauiContext context, IItemsLayout itemsLayout)
	{
		var items = new ObservableCollection<string>(Enumerable.Range(0, ItemCount).Select(i => $"Item {i}"));
		var collectionView = new CollectionView
		{
			ItemsLayout = itemsLayout,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 8 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
			ItemsSource = items,
		};

		var platformView = collectionView.ToPlatform(context);

		// A detached RecyclerView never runs its item animations, so measure the layout work only
		if (platformView is RecyclerView recyclerView)
			recyclerView.SetItemAnimator(null);

		LayoutPass(platformView);
		return (items, platformView);
	}

	static void LayoutPass(Android.Views.View view)
	{
		view.Measure(
			Android.Views.View.MeasureSpec.MakeMeasureSpec(Width, MeasureSpecMode.Exactly),
			Android.Views.View.MeasureSpec.MakeMeasureSpec(Height, MeasureSpecMode.Exactly));
		view.Layout(0, 0, Width, Height);
	}

	static void RunOnMain(Action action) =>
		MainInstrumentation.Instance!.RunOnMainSync(new Java.Lang.Runnable(action));
}
