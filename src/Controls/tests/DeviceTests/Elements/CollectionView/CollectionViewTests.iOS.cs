using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		[Fact]
		public async Task ItemsSourceGroupedClearDoestCrash()
		{
			SetupBuilder();

			var data = new List<string> { "test 1", "test 2", "test 3" };
			var groupData = new ObservableCollection<CollectionViewStringGroup>
				{
					new ("Header 1", data),
					new ("Header 2", data),
					new ("Header 3", data)
				};

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groupData,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await Task.Delay(1000);
				groupData.Clear();
				groupData.Add(new("Header 1", new string[] { "oi" }));
			});
		}

		// Regression tests for https://github.com/dotnet/maui/issues/34691
		//
		// Root cause: ObservableGroupedSource.CollectionChanged() called UpdateSection() before
		// processing Remove. INCC semantics require the item to be already removed from the
		// backing collection before CollectionChanged fires, so _groupSource has N-1 items when
		// MAUI receives Remove — but UIKit still has N sections. UpdateSection() then iterates
		// UIKit's N sections and calls NumberOfItemsInSection(N-1), re-entering the data source:
		//   GetGroupCount(N-1) → _groupSource[N-1] → ArgumentOutOfRangeException.
		//
		// These tests use CollectionViewHandler2 (the default iOS handler) because the production
		// crash in issue #34691 goes through ItemsViewController2 (Items2/), which calls
		// ItemsSourceFactory.CreateGrouped() → ObservableGroupedSource (Items/). Both handlers
		// share the same ObservableGroupedSource, so fixing it covers both paths.
		//
		// Each test forces a synchronous UIKit layout pass after the removal via SetNeedsLayout()
		// + LayoutIfNeeded(). Before the fix this deterministically triggers the crash:
		//   CollectionChanged(Remove) → UpdateSection() → NumberOfItemsInSection(N-1)
		//   → GetGroupCount(N-1) → ArgumentOutOfRangeException.
		// After the fix the pre-Remove UpdateSection() is skipped and GetGroupCount() has a
		// defensive bounds check, so both the synchronous and the deferred layout paths are safe.

		[Fact(DisplayName = "Removing last section from grouped CollectionView does not crash (Handler2)")]
		public async Task ItemsSourceGroupedRemoveLastSectionDoesNotCrash()
		{
			SetupBuilder();

			var data = new List<string> { "item 1", "item 2", "item 3" };
			var groupData = new ObservableCollection<CollectionViewStringGroup>
			{
				new("Header 1", data),
				new("Header 2", data),
				new("Header 3", data),
			};

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groupData,
				ItemTemplate = new DataTemplate(() => new Label()),
			};

			// Capture the pre-layout frame so WaitForUIUpdate can detect the first layout pass.
			var initialFrame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				// Wait for the initial layout rather than a fixed delay.
				await WaitForUIUpdate(initialFrame, collectionView);

				// Removing the last section was the classic crash: UIKit still has N sections
				// when UpdateSection() is called before Remove(), so GetGroupCount(N-1) is
				// invoked against a _groupSource that already has N-1 items → out of range.
				groupData.RemoveAt(groupData.Count - 1);

				// Force a synchronous layout pass so UIKit queries the data source immediately.
				// Without the fix this deterministically throws ArgumentOutOfRangeException.
				handler.PlatformView.SetNeedsLayout();
				handler.PlatformView.LayoutIfNeeded();

				Assert.Equal(2, groupData.Count);
			});
		}

		[Fact(DisplayName = "Removing first section from grouped CollectionView does not crash (Handler2)")]
		public async Task ItemsSourceGroupedRemoveFirstSectionDoesNotCrash()
		{
			SetupBuilder();

			var data = new List<string> { "item 1", "item 2" };
			var groupData = new ObservableCollection<CollectionViewStringGroup>
			{
				new("Header 1", data),
				new("Header 2", data),
				new("Header 3", data),
			};

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groupData,
				ItemTemplate = new DataTemplate(() => new Label()),
			};

			var initialFrame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				await WaitForUIUpdate(initialFrame, collectionView);

				groupData.RemoveAt(0);
				handler.PlatformView.SetNeedsLayout();
				handler.PlatformView.LayoutIfNeeded();

				groupData.RemoveAt(0);
				handler.PlatformView.SetNeedsLayout();
				handler.PlatformView.LayoutIfNeeded();

				Assert.Single(groupData);
			});
		}

		[Fact(DisplayName = "Removing all sections one by one from grouped CollectionView does not crash (Handler2)")]
		public async Task ItemsSourceGroupedRemoveAllSectionsOneByOneDoesNotCrash()
		{
			SetupBuilder();

			var data = new List<string> { "item 1", "item 2" };
			var groupData = new ObservableCollection<CollectionViewStringGroup>
			{
				new("Header 1", data),
				new("Header 2", data),
				new("Header 3", data),
			};

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groupData,
				ItemTemplate = new DataTemplate(() => new Label()),
			};

			var initialFrame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				await WaitForUIUpdate(initialFrame, collectionView);

				while (groupData.Count > 0)
				{
					groupData.RemoveAt(groupData.Count - 1);
					// Force synchronous layout after each removal to exercise the full range
					// of section indices and confirm no re-entrancy crash occurs.
					handler.PlatformView.SetNeedsLayout();
					handler.PlatformView.LayoutIfNeeded();
				}

				Assert.Empty(groupData);
			});
		}

		class CollectionViewStringGroup : List<string>
		{
			public string GroupHeader { get; private set; }
			public CollectionViewStringGroup(string header, IEnumerable<string> data) : base(data)
			{
				GroupHeader = header;
			}
		}

		[Fact]
		public async Task CollectionViewItemsArrangeCorrectly()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = Enumerable.Range(1, 50).Select(i => $"Item {i}").ToArray();

			var collectionView = new CollectionView
			{
				ItemsSource = items,
				MaximumHeightRequest = 300,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
					};
					label.SetBinding(Label.TextProperty, ".");

					return new Border
					{
						WidthRequest = 200,
						StrokeShape = new RoundRectangle() { CornerRadius = 12 },
						Content = label
					};
				}),
				ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					HorizontalItemSpacing = 50
				}
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, handler =>
			{
				// Get the first cell's content
				var firstCellContent = collectionView.GetVisualTreeDescendants().OfType<Border>().FirstOrDefault();

				Assert.NotNull(firstCellContent);

				var frame = firstCellContent.Frame;

				Assert.True(frame.Width == 200 && frame.Height == 300);

			});
		}

		[Fact]
		public async Task CollectionViewContentRespectsMargin()
		{
			SetupBuilder();

			// We'll use an EmptyView to assess whether the CollectionView's content 
			// is being properly offset by the margin
			var emptyView = new VerticalStackLayout();
			var emptyViewContent = new Label { Text = "test" };
			emptyView.Add(emptyViewContent);

			double margin = 2;

			var collectionView = new CollectionView
			{
				Margin = new Thickness(margin),
				EmptyView = emptyView,
			};

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);

				if (emptyViewContent.Handler.PlatformView is not UIView nativeLabel)
				{
					throw new XunitException("EmptyView Content is not a UIView");
				}

				var point = new CGPoint(nativeLabel.Frame.Left, nativeLabel.Frame.Top);

				// Convert the local point to an absolute point in the window 
				var absPoint = nativeLabel.ConvertPointToView(point, null);

				Assert.Equal(margin, absPoint.X);
			});
		}

		[Fact("Cells Do Not Leak")]
		public async Task CellsDoNotLeak()
		{
			SetupBuilder();

			var labels = new List<WeakReference>();
			VerticalCell cell = null;

			var bindingContext = "foo";
			var collectionView = new MyUserControl
			{
				Labels = labels
			};
			collectionView.ItemTemplate = new DataTemplate(collectionView.LoadDataTemplate);

			var handler = await CreateHandlerAsync(collectionView);

			await InvokeOnMainThreadAsync(() =>
			{
				cell = new VerticalCell(CGRect.Empty);
				cell.Bind(collectionView.ItemTemplate, bindingContext, collectionView);
			});

			Assert.NotNull(cell);

			// HACK: test passes running individually, but fails when running entire suite.
			// Skip the assertion on Catalyst for now.
#if !MACCATALYST
			await AssertionExtensions.WaitForGC([.. labels]);
#endif
		}

		//src/Compatibility/Core/tests/iOS/ObservableItemsSourceTests.cs
		[Fact(DisplayName = "IndexPath Range Generation Is Correct")]
		public void GenerateIndexPathRange()
		{
			SetupBuilder();

			var result = IndexPathHelpers.GenerateIndexPathRange(0, 0, 5);

			Assert.Equal(5, result.Length);

			Assert.Equal(0, result[0].Section);
			Assert.Equal(0, (int)result[0].Item);

			Assert.Equal(0, result[4].Section);
			Assert.Equal(4, (int)result[4].Item);
		}

		//src/Compatibility/Core/tests/iOS/ObservableItemsSourceTests.cs
		[Fact(DisplayName = "IndexPath Range Generation For Loops Is Correct")]
		public void GenerateIndexPathRangeForLoop()
		{
			SetupBuilder();

			var result = IndexPathHelpers.GenerateLoopedIndexPathRange(0, 15, 3, 2, 3);

			Assert.Equal(9, result.Length);

			for (int i = 0; i < result.Length; i++)
			{
				Assert.Equal(0, result[i].Section);
			}

			Assert.Equal(2, (int)result[0].Item);
			Assert.Equal(3, (int)result[1].Item);
			Assert.Equal(4, (int)result[2].Item);

			Assert.Equal(7, (int)result[3].Item);
			Assert.Equal(8, (int)result[4].Item);
			Assert.Equal(9, (int)result[5].Item);

			Assert.Equal(12, (int)result[6].Item);
			Assert.Equal(13, (int)result[7].Item);
			Assert.Equal(14, (int)result[8].Item);
		}

		//src/Compatibility/Core/tests/iOS/ObservableItemsSourceTests.cs
		[Fact(DisplayName = "IndexPath Validity Check Is Correct")]
		public void IndexPathValidTest()
		{
			var list = new List<string>
			{
				"one",
				"two",
				"three"
			};

			var source = new ListSource((IEnumerable<object>)list);

			var valid = NSIndexPath.FromItemSection(2, 0);
			var invalidItem = NSIndexPath.FromItemSection(7, 0);
			var invalidSection = NSIndexPath.FromItemSection(1, 9);

			Assert.True(source.IsIndexPathValid(valid));
			Assert.False(source.IsIndexPathValid(invalidItem));
			Assert.False(source.IsIndexPathValid(invalidSection));
		}

		[Fact(DisplayName = "CollectionView Does Not Leak With Default ItemsLayout")]
		public async Task CollectionViewDoesNotLeakWithDefaultItemsLayout()
		{
			SetupBuilder();

			WeakReference weakCollectionView = null;
			WeakReference weakHandler = null;

			await InvokeOnMainThreadAsync(async () =>
			{
				var collectionView = new CollectionView
				{
					ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
					ItemTemplate = new DataTemplate(() => new Label())
					// Note: Not setting ItemsLayout - using the default
				};

				weakCollectionView = new WeakReference(collectionView);

				var handler = await CreateHandlerAsync<CollectionViewHandler2>(collectionView);

				// Verify handler is created
				Assert.NotNull(handler);

				// Store weak reference to the handler
				weakHandler = new WeakReference(handler);

				// Disconnect the handler
				((IElementHandler)handler).DisconnectHandler();
			});

			// Force garbage collection
			await AssertionExtensions.WaitForGC(weakCollectionView, weakHandler);

			// Verify the CollectionView was collected
			Assert.False(weakCollectionView.IsAlive, "CollectionView should have been garbage collected");

			// Verify the handler was collected
			Assert.False(weakHandler.IsAlive, "CollectionViewHandler2 should have been garbage collected");
		}

		/// <summary>
		/// Simulates what a developer might do with a Page/View
		/// </summary>
		class MyUserControl : CollectionView
		{
			public List<WeakReference> Labels { get; set; }

			/// <summary>
			/// Used for reproducing a leak w/ instance methods on ItemsView.ItemTemplate
			/// </summary>
			public object LoadDataTemplate()
			{
				var label = new Label();
				Labels.Add(new(label));
				return label;
			}
		}

		Rect GetCollectionViewCellBounds(IView cellContent)
		{
			if (!cellContent.ToPlatform().IsLoaded())
			{
				throw new System.Exception("The cell is not in the visual tree");
			}

			return cellContent.ToPlatform().GetParentOfType<UIKit.UICollectionViewCell>().GetBoundingBox();
		}

		// Regression test for https://github.com/dotnet/maui/issues/34635
		[Fact("CollectionView cell MauiViews should be treated as UIScrollView descendants and not apply safe area independently")]
		[Category(TestCategory.CollectionView)]
		public async Task CollectionViewCellContentShouldBeScrollViewDescendant()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = Enumerable.Range(0, 5).Select(i => $"Item {i}").ToList(),
				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid { Padding = new Thickness(10) };
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					grid.Add(label);
					return grid;
				}),
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await Task.Delay(500);

				var platformCV = collectionView.ToPlatform();
				Assert.NotNull(platformCV);

				var uiCollectionView = platformCV as UICollectionView
					?? platformCV.GetParentOfType<UICollectionView>();

				if (uiCollectionView is null && platformCV is UIView pv)
					uiCollectionView = pv.Subviews.OfType<UICollectionView>().FirstOrDefault();

				Assert.NotNull(uiCollectionView);

				var visibleCells = uiCollectionView.VisibleCells;
				Assert.NotEmpty(visibleCells);

				foreach (var cell in visibleCells)
				{
					foreach (var mv in FindAllSubviews<MauiView>(cell))
					{
						mv.SetNeedsLayout();
						mv.LayoutIfNeeded();

						Assert.False(mv.AppliesSafeAreaAdjustments,
							$"CollectionView cell MauiView '{mv.View?.GetType().Name}' should not apply safe area adjustments. " +
							"Cell views inside UICollectionView must be treated as scroll view descendants.");
					}
				}
			});
		}

		static List<T> FindAllSubviews<T>(UIView root) where T : UIView
		{
			var result = new List<T>();
			foreach (var subview in root.Subviews)
			{
				if (subview is T match)
					result.Add(match);
				result.AddRange(FindAllSubviews<T>(subview));
			}
			return result;
		}

		[Fact(DisplayName = "CarouselView ScrollBar Visibility should Update")]
		public async Task CheckCarouselViewScrollBarVisibilityUpdates()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var carouselView = new CarouselView
			{
				ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" },
				ItemTemplate = new DataTemplate(() => new Label { WidthRequest = 200 })
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, async handler =>
			{
				await Task.Delay(100); // Allow layout to complete
				var nativeCollectionView = handler.Controller?.CollectionView;
				Assert.NotNull(nativeCollectionView);

				// CarouselView should use CompositionalLayout
				Assert.IsType<UICollectionViewCompositionalLayout>(nativeCollectionView.CollectionViewLayout);

				// Test ScrollBarVisibility.Always
				carouselView.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
				carouselView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
				await Task.Delay(100); // Allow BeginInvokeOnMainThread callbacks to drain

				// Poll for the internal scroll view (it may not be created synchronously)
				UIScrollView internalScrollView = null;
				for (int attempt = 0; attempt < 10 && internalScrollView == null; attempt++)
				{
					internalScrollView = FindInternalScrollView(nativeCollectionView);
					if (internalScrollView == null)
						await Task.Delay(100);
				}
				Assert.NotNull(internalScrollView); // Must exist for the test to be valid

				Assert.True(internalScrollView.ShowsHorizontalScrollIndicator);
				Assert.True(internalScrollView.ShowsVerticalScrollIndicator);
				Assert.True(nativeCollectionView.ShowsHorizontalScrollIndicator);
				Assert.True(nativeCollectionView.ShowsVerticalScrollIndicator);

				// Test ScrollBarVisibility.Never
				carouselView.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
				carouselView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
				await Task.Delay(100);

				Assert.False(internalScrollView.ShowsHorizontalScrollIndicator); // Key assertion for this bug!
				Assert.False(internalScrollView.ShowsVerticalScrollIndicator);
				Assert.False(nativeCollectionView.ShowsHorizontalScrollIndicator);
				Assert.False(nativeCollectionView.ShowsVerticalScrollIndicator);
			});
		}

		private static UIScrollView FindInternalScrollView(UICollectionView collectionView)
		{
			// In CV2 the scroll indicators are managed by an internal UIScrollView.
			foreach (var subview in collectionView.Subviews)
			{
				if (subview is UIScrollView scrollView && scrollView != collectionView)
				{
					return scrollView;
				}
			}
			return null;
		}
	}
}