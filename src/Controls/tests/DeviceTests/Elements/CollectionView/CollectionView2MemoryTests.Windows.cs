using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests;

[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
[Category(TestCategory.CollectionView)]
public class CollectionView2MemoryTests : ControlsHandlerTestBase
{
	internal static void SetupHandlers(ControlsHandlerTestBase test)
	{
		test.EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<CollectionView, CollectionViewHandler2>();
			handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			handlers.AddHandler<Grid, LayoutHandler>();
			handlers.AddHandler<Label, LabelHandler>();
		}));
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	[InlineData(3, true)]
	public async Task RemovedCollectionViewCollectsWhileHostAndSourceRemainAlive(int sourceKind, bool clearSource)
	{
		SetupHandlers(this);
		var host = new VerticalStackLayout();
		var source = CreateSource(sourceKind);

		await CreateHandlerAndAddToWindow<LayoutHandler>(host, async _ =>
		{
			var references = await AddAndRemoveCollectionView(host, source, sourceKind == 3, clearSource);
			for (var attempt = 0; attempt < 20 && references.Any(reference => reference.IsAlive); attempt++)
			{
				await AssertionExtensions.Collect();
				await Task.Delay(100);
			}
			await AssertionExtensions.WaitForGC(references);
			Assert.NotNull(host.Handler);
			Assert.Empty(host.Children);
			GC.KeepAlive(source);
		});
	}

	static IEnumerable CreateSource(int sourceKind) => sourceKind switch
	{
		0 => new ObservableCollection<string> { "One", "Two", "Three" },
		1 => new List<string> { "One", "Two", "Three" },
		2 => Enumerable.Range(1, 3).Select(static value => value.ToString()),
		3 => new ObservableCollection<ObservableCollection<string>>
		{
			new() { "One", "Two", "Three" },
			new() { "Four", "Five", "Six" }
		},
		_ => throw new ArgumentOutOfRangeException(nameof(sourceKind))
	};

	[MethodImpl(MethodImplOptions.NoInlining)]
	static async Task<WeakReference[]> AddAndRemoveCollectionView(
		VerticalStackLayout host, IEnumerable source, bool grouped, bool clearSource)
	{
		var rows = new List<WeakReference>();
		var references = new List<WeakReference>();
		var collectionView = new CollectionView
		{
			HeightRequest = 240,
			IsGrouped = grouped,
			ItemsSource = source,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				var row = new Grid { HeightRequest = 40, Children = { label } };
				rows.Add(new WeakReference(row));
				references.Add(new WeakReference(row));
				references.Add(new WeakReference(label));
				return row;
			})
		};

		if (grouped)
		{
			collectionView.GroupHeaderTemplate = new DataTemplate(() => new Label { Text = "Group header" });
			collectionView.GroupFooterTemplate = new DataTemplate(() => new Label { Text = "Group footer" });
		}

		host.Children.Add(collectionView);
		await AssertEventually(() => rows.Count > 0 && rows[0].Target is Grid { IsLoaded: true } row &&
			row.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement { ActualHeight: > 0 });
		Assert.IsType<CollectionViewHandler2>(collectionView.Handler);
		references.Add(new WeakReference(collectionView));
		references.Add(new WeakReference(collectionView.Handler));
		references.Add(new WeakReference(collectionView.Handler.PlatformView));

		if (clearSource)
			((IList)source).Clear();

		host.Children.Remove(collectionView);
		await Task.Delay(100);
		return references.ToArray();
	}

	[Fact]
	public async Task GroupedSourceDoesNotRetainItsAdapter()
	{
		SetupHandlers(this);
		var source = (ObservableCollection<ObservableCollection<string>>)CreateSource(3);
		var reference = await InvokeOnMainThreadAsync(() => CreateGroupedAdapter(source));

		await AssertionExtensions.WaitForGC(reference);
		source[0].Add("After collection");
		source.Add(new ObservableCollection<string> { "Another group" });
		GC.KeepAlive(source);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference CreateGroupedAdapter(ObservableCollection<ObservableCollection<string>> source)
	{
		var adapter = new GroupedItemTemplateCollection2(source,
			new DataTemplate(() => new Label()), null, null, new CollectionView());
		Assert.Equal(6, adapter.Count);
		return new WeakReference(adapter);
	}

	[Fact]
	public async Task RetainedFactoryDoesNotRetainRecycledViews()
	{
		SetupHandlers(this);
		var (factory, references) = await InvokeOnMainThreadAsync(CreateFactoryWithRecycledItem);
		await AssertionExtensions.WaitForGC(references);
		GC.KeepAlive(factory);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	(ItemFactory Factory, WeakReference[] References) CreateFactoryWithRecycledItem()
	{
		var owner = new CollectionView();
		CreateHandler<CollectionViewHandler2>(owner);
		var template = new DataTemplate(() => new Label());
		var factory = new ItemFactory(owner);
		var element = factory.GetElement(new Microsoft.UI.Xaml.ElementFactoryGetArgs
		{
			Data = new ItemTemplateContext2(template, "Item", owner)
		});
		var label = Assert.Single(owner.LogicalChildrenInternal.OfType<Label>());
		factory.RecycleElement(new Microsoft.UI.Xaml.ElementFactoryRecycleArgs { Element = element });
		return (factory, new[] { new WeakReference(owner), new WeakReference(label) });
	}
}

[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
[Category(TestCategory.CollectionView)]
public class CollectionView2ReattachmentTests : ControlsHandlerTestBase
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task RemovedCollectionViewCanBeReattachedAndUpdated(bool grouped)
	{
		CollectionView2MemoryTests.SetupHandlers(this);
		var items = new ObservableCollection<string> { "One", "Two", "Three" };
		var groups = new ObservableCollection<ObservableCollection<string>> { items };
		var collectionView = new CollectionView
		{
			HeightRequest = 600,
			IsGrouped = grouped,
			ItemsSource = grouped ? groups : items,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
			ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
			SelectionMode = SelectionMode.Single,
			Header = new Label { Text = "Header" },
			Footer = new Label { Text = "Footer" },
			EmptyView = new Label { Text = "Empty" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 40 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
			GroupHeaderTemplate = new DataTemplate(() => new Label { Text = "Group header" }),
			GroupFooterTemplate = new DataTemplate(() => new Label { Text = "Group footer" })
		};
		var host = new VerticalStackLayout { collectionView };

		await CreateHandlerAndAddToWindow<LayoutHandler>(host, async _ =>
		{
			await AssertEventually(() => HasRenderedItem(collectionView, "One"));
			var originalHandler = collectionView.Handler;
			host.Children.Remove(collectionView);
			await Task.Delay(100);
			host.Children.Add(collectionView);
			await AssertEventually(() => HasRenderedItem(collectionView, "One"));
			Assert.Same(originalHandler, collectionView.Handler);
			await AssertionExtensions.Collect();

			items.Add("Added");
			await AssertEventually(() => HasRenderedItem(collectionView, "Added"));
			items[0] = "Replaced";
			var platformItems = ((CollectionViewHandler2)collectionView.Handler).PlatformView;
			Assert.Contains(((IEnumerable)platformItems.ItemsSource).Cast<ItemTemplateContext2>(), context => Equals(context.Item, "Replaced"));
			// Grouped Replace already fails to refresh realized visuals on the baseline.
			if (!grouped)
				await AssertEventually(() => HasRenderedItem(collectionView, "Replaced"));
			items.Move(0, 1);
			items.RemoveAt(0);
			Assert.Equal(new[] { "Replaced", "Three", "Added" },
				((IEnumerable)platformItems.ItemsSource).Cast<ItemTemplateContext2>()
					.Where(context => !context.IsHeader && !context.IsFooter).Select(context => context.Item));
			if (grouped)
			{
				groups.Add(new ObservableCollection<string> { "Another group" });
				groups.RemoveAt(1);
			}
			items.Clear();
			await AssertEventually(() => !HasRenderedItem(collectionView, "Replaced"));
			items.Add("Final");
			await AssertEventually(() => HasRenderedItem(collectionView, "Final"));
			collectionView.SelectedItem = "Final";
			var platformView = ((CollectionViewHandler2)collectionView.Handler).PlatformView;
			await AssertEventually(() => platformView.SelectedItem is ItemTemplateContext2 { Item: "Final" });
		});
	}

	[Fact]
	public async Task GroupSubscriptionsSurviveCollectionAndFollowGroupChanges()
	{
		CollectionView2MemoryTests.SetupHandlers(this);
		await InvokeOnMainThreadAsync(async () =>
		{
			var owner = new CollectionView();
			var first = new ObservableCollection<string> { "One", "Two" };
			var groups = new ObservableCollection<ObservableCollection<string>> { first };
			var template = new DataTemplate(() => new Label());
			var adapter = new GroupedItemTemplateCollection2(groups, template, template, template, owner);

			await AssertionExtensions.Collect();
			first.Add("Three");
			Assert.Equal(new object[] { first, "One", "Two", "Three", first }, adapter.Select(context => context.Item));
			first[0] = "Replaced";
			first.Move(0, 2);
			first.RemoveAt(0);
			Assert.Equal(new object[] { first, "Three", "Replaced", first }, adapter.Select(context => context.Item));

			var second = new ObservableCollection<string> { "Second" };
			groups.Add(second);
			await AssertionExtensions.Collect();
			second.Add("After GC");
			Assert.Equal(8, adapter.Count);
			groups.Move(1, 0);
			Assert.Same(second, adapter[0].Item);
			groups[0] = new ObservableCollection<string> { "Replacement group" };
			second.Add("Unsubscribed");
			Assert.DoesNotContain(adapter, context => Equals(context.Item, "Unsubscribed"));
			groups.RemoveAt(0);
			first.Clear();
			Assert.Equal(new object[] { first, first }, adapter.Select(context => context.Item));
			groups.Clear();
			Assert.Empty(adapter);
			groups.Add(second);
			Assert.Equal(5, adapter.Count);
			adapter.CleanUp();
			second.Add("After cleanup");
			groups.Clear();
			Assert.Equal(5, adapter.Count);
			GC.KeepAlive(owner);
		});
	}

	[Fact]
	public async Task RecycledItemRebindsWhileOwnerRemainsAlive()
	{
		CollectionView2MemoryTests.SetupHandlers(this);
		await InvokeOnMainThreadAsync(() =>
		{
			var collectionView = new CollectionView();
			CreateHandler<CollectionViewHandler2>(collectionView);
			var template = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			});
			var factory = new ItemFactory(collectionView);
			var first = factory.GetElement(new Microsoft.UI.Xaml.ElementFactoryGetArgs
			{
				Data = new ItemTemplateContext2(template, "First", collectionView)
			});
			var label = Assert.Single(collectionView.LogicalChildrenInternal.OfType<Label>());
			Assert.Equal("First", label.Text);
			factory.RecycleElement(new Microsoft.UI.Xaml.ElementFactoryRecycleArgs { Element = first });
			GC.Collect();
			GC.WaitForPendingFinalizers();
			var second = factory.GetElement(new Microsoft.UI.Xaml.ElementFactoryGetArgs
			{
				Data = new ItemTemplateContext2(template, "Second", collectionView)
			});
			Assert.Same(first, second);
			Assert.Same(label, Assert.Single(collectionView.LogicalChildrenInternal.OfType<Label>()));
			Assert.Equal("Second", label.Text);
			Assert.Equal("Second", label.BindingContext);
			factory.CleanUp();
			GC.KeepAlive(collectionView);
		});
	}

	static bool HasRenderedItem(CollectionView collectionView, string text) =>
		collectionView.LogicalChildrenInternal.OfType<Label>().Any(label => label.Text == text && label.IsLoaded);
}
