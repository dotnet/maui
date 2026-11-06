using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
#if IOS || MACCATALYST
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using Xunit.Sdk;
#endif

namespace Microsoft.Maui.DeviceTests;

public partial class Maui34499ItemListView : ContentView
{
	public Maui34499ItemListView() => InitializeComponent();

	public VerticalStackLayout ItemStack => ItemsLayout;
	public Label TotalLabel => CountLabel;
}

#if IOS || MACCATALYST
[Category(TestCategory.Layout)]
public class Issue34499NativeLayoutTests : ControlsHandlerTestBase
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task SourceGenContentViewRendersItemsAfterDelayedBindingContextAndCollectionAdd(bool bindItemsSource)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<ContentPage, PageHandler>();
				handlers.AddHandler<ScrollView, ScrollViewHandler>();
				handlers.AddHandler<Layout, LayoutHandler>();
				handlers.AddHandler<ContentView, ContentViewHandler>();
				handlers.AddHandler<Border, BorderHandler>();
				handlers.AddHandler<Label, LabelHandler>();
			});
		});

		await InvokeOnMainThreadAsync(async () =>
		{
			// rmarinho/bindable-layout-repro@203cda56f9de609e67347b02cf50f795db994cd5:
			// MainPage -> ScrollView -> stack -> header + ItemListView.
			var list = new Maui34499ItemListView();
			var page = new ContentPage
			{
				// AttachAndRun measures the root with infinite constraints, unlike an app window.
				WidthRequest = 600,
				HeightRequest = 800,
				Content = new ScrollView
				{
					Content = new VerticalStackLayout
					{
						Padding = 30,
						Spacing = 20,
						Children =
						{
							new Label { Text = "BindableLayout reproduction" },
							list
						}
					}
				}
			};
			var model = new ItemsViewModel();

			// Deliberately assign only to the ContentView, AFTER its generated InitializeComponent
			// and after construction of the page tree, just like MainPage's constructor.
			Assert.Null(list.BindingContext);
			if (!bindItemsSource)
				list.ItemStack.RemoveBinding(BindableLayout.ItemsSourceProperty);
			list.BindingContext = model;
			if (!bindItemsSource)
				BindableLayout.SetItemsSource(list.ItemStack, model.Items);

			await AttachAndRun(page, async _ =>
			{
				await WaitForNativeItems(list, model.Items);

				// Mutate only after the initial five children have real, non-overlapping native frames.
				model.Items.Add("Item 6 - Fig");
				await WaitForNativeItems(list, model.Items);
			});
		});
	}

	static async Task WaitForNativeItems(Maui34499ItemListView list, ObservableCollection<string> items)
	{
		// Retry assertions, not a forced Measure/Arrange or ToPlatform: neither should repair
		// missing handlers/layout on behalf of the production invalidation path.
		// On timeout, run the assertions once more below to retain the specific failing
		// text/frame/attachment assertion in CI rather than a generic timeout message.
		await AssertHelpers.Wait(() =>
		{
			try
			{
				AssertNativeItems(list, items);
				return true;
			}
			catch (XunitException)
			{
				return false;
			}
		}, timeout: 5000);

		AssertNativeItems(list, items);
	}

	static void AssertNativeItems(Maui34499ItemListView list, ObservableCollection<string> items)
	{
		var nativeList = Assert.IsAssignableFrom<UIView>(list.Handler?.PlatformView);
		var window = nativeList.Window;
		Assert.NotNull(window);
		var listFrame = AssertRendered(nativeList, window);
		var nativeStack = Assert.IsAssignableFrom<UIView>(list.ItemStack.Handler?.PlatformView);
		Assert.True(nativeStack.IsDescendantOfView(nativeList), "Item stack must be in the ContentView's native subtree.");
		AssertRendered(nativeStack, window);

		// Inspect actual UIKit descendants, rather than merely trusting managed Labels/Children.
		var nativeLabels = Descendants(nativeStack).OfType<UILabel>().ToArray();
		Assert.Equal(items.ToArray(), nativeLabels.Select(label => label.Text).ToArray());
		Assert.Equal(items.Count, list.ItemStack.Children.Count);

		CGRect? previousFrame = null;
		for (var i = 0; i < items.Count; i++)
		{
			var border = Assert.IsType<Border>(list.ItemStack.Children[i]);
			var nativeBorder = Assert.IsAssignableFrom<UIView>(border.Handler?.PlatformView);
			Assert.True(nativeBorder.IsDescendantOfView(nativeStack), $"Item {i} border is missing from the native stack.");
			var borderFrame = AssertRendered(nativeBorder, window);
			var nativeLabel = Assert.Single(Descendants(nativeBorder).OfType<UILabel>());
			Assert.Same(nativeLabels[i], nativeLabel);
			Assert.Equal(items[i], nativeLabel.Text);
			var labelFrame = AssertRendered(nativeLabel, window);
			Assert.True(labelFrame.Left >= borderFrame.Left - 0.5 && labelFrame.Right <= borderFrame.Right + 0.5
				&& labelFrame.Top >= borderFrame.Top - 0.5 && labelFrame.Bottom <= borderFrame.Bottom + 0.5,
				$"Item {i} label {labelFrame} must fit inside its native border {borderFrame}.");
			Assert.True(borderFrame.Top >= listFrame.Top - 0.5 && borderFrame.Bottom <= listFrame.Bottom + 0.5,
				$"Item {i} border {borderFrame} must fit inside the native ContentView {listFrame}.");

			if (previousFrame is CGRect previous)
			{
				Assert.True(borderFrame.Top >= previous.Bottom + 3,
					$"Item {i} native frame {borderFrame} overlaps/precedes {previous}; expected Spacing=4.");
			}

			previousFrame = borderFrame;
		}

		var nativeCount = Assert.IsAssignableFrom<UILabel>(list.TotalLabel.Handler?.PlatformView);
		Assert.True(nativeCount.IsDescendantOfView(nativeList));
		Assert.Equal($"Total items: {items.Count}", nativeCount.Text);
		var countFrame = AssertRendered(nativeCount, window);
		Assert.True(countFrame.Top >= previousFrame.Value.Bottom,
			$"Native count label {countFrame} must follow the last item {previousFrame}.");
	}

	static CGRect AssertRendered(UIView view, UIWindow window)
	{
		Assert.Same(window, view.Window);
		for (var ancestor = view; ancestor != null; ancestor = ancestor.Superview)
		{
			Assert.False(ancestor.Hidden);
			Assert.True(ancestor.Alpha > 0);
		}

		var frame = view.ConvertRectToView(view.Bounds, window);
		Assert.True(frame.Width > 0 && frame.Height > 0, $"Expected a nonzero native frame, got {frame} for {view}.");
		return frame;
	}

	static IEnumerable<UIView> Descendants(UIView view)
	{
		foreach (var child in view.Subviews)
		{
			yield return child;
			foreach (var descendant in Descendants(child))
				yield return descendant;
		}
	}

	sealed class ItemsViewModel
	{
		public ObservableCollection<string> Items { get; } = new()
		{
			"Item 1 - Apple",
			"Item 2 - Banana",
			"Item 3 - Cherry",
			"Item 4 - Date",
			"Item 5 - Elderberry"
		};
	}
}
#endif
