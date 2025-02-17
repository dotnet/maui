﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;
using Foundation;
using Microsoft.Maui.Handlers;

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
			SetupBuilder();

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

			{
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
			}

			// HACK: test passes running individually, but fails when running entire suite.
			// Skip the assertion on Catalyst for now.
#if !MACCATALYST
			await AssertionExtensions.WaitForGC(labels.ToArray());
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
	}
}