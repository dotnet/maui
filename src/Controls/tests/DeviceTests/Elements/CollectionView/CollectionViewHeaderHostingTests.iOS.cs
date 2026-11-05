using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, false)]
		[InlineData(true, true, false)]
		[InlineData(false, false, true)]
		[InlineData(false, true, true)]
		[InlineData(true, false, true)]
		[InlineData(true, true, true)]
		public async Task GlobalSupplementaryReplacementWithNoGroupsRemovesOldContent(bool inFooter, bool useTemplate, bool hidden)
		{
			SetupBuilder();
			var groups = new ObservableCollection<ObservableCollection<string>> { new() { "One" } };
			Label content = null;
			var contentProperty = inFooter ? StructuredItemsView.FooterProperty : StructuredItemsView.HeaderProperty;
			var templateProperty = inFooter ? StructuredItemsView.FooterTemplateProperty : StructuredItemsView.HeaderTemplateProperty;
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				IsGrouped = true,
				ItemsSource = groups,
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 })
			};
			if (useTemplate)
				collectionView.SetValue(templateProperty, new DataTemplate(() => content = new Label { Text = "Old" }));
			else
				collectionView.SetValue(contentProperty, content = new Label { Text = "Old" });

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => content?.Handler?.PlatformView is UIView { Window: not null });
				var oldContent = content;
				var oldNative = (UIView)oldContent.Handler.PlatformView;
				groups.Clear();
				collection.LayoutIfNeeded();
				await AssertEventually(() => collection.NumberOfSections() == 0);
				if (hidden)
				{
					collectionView.IsVisible = false;
					await AssertEventually(() => collection.Hidden);
				}

				if (useTemplate)
					collectionView.SetValue(templateProperty, new DataTemplate(() => content = new Label { Text = "New" }));
				else
					collectionView.SetValue(contentProperty, content = new Label { Text = "New" });
				collection.LayoutIfNeeded();
				Assert.Null(oldContent.Parent);
				Assert.Null(oldNative.Window);

				groups.Add(new() { "Two" });
				collectionView.IsVisible = true;
				collection.LayoutIfNeeded();
				await AssertEventually(() => content != oldContent && content?.Handler?.PlatformView is UIView { Window: not null });
				Assert.Equal("New", content.Text);
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task GlobalSupplementaryRuntimeContainerSurvivesReload(bool inFooter, bool useClip)
		{
			SetupBuilder();
			Label content = null;
			var items = new ObservableCollection<string> { "One" };
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 })
			};
			collectionView.SetValue(inFooter ? StructuredItemsView.FooterTemplateProperty : StructuredItemsView.HeaderTemplateProperty,
				new DataTemplate(() => content = new Label { Text = "Content", HeightRequest = 50 }));

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => content?.Handler?.PlatformView is UIView { Window: not null });
				var platform = (UIView)content.Handler.PlatformView;
				if (useClip)
					content.Clip = new RectangleGeometry { Rect = new Rect(0, 0, 80, 40) };
				else
					content.Shadow = new Shadow { Brush = Brush.Black, Radius = 8, Offset = new Point(2, 2) };
				var container = Assert.IsAssignableFrom<UIView>(((IViewHandler)content.Handler).ContainerView);
				Assert.Same(container, platform.Superview);

				items.Clear();
				items.Add("Two");
				collection.LayoutIfNeeded();
				await AssertEventually(() => collection.NumberOfItemsInSection(0) == 1);
				Assert.Same(container, platform.Superview);
				Assert.NotNull(container.Window);
				var cell = Assert.IsAssignableFrom<TemplatedCell2>(collection.ViewWithTag(
					inFooter ? StructuredItemsViewController2<ReorderableItemsView>.FooterTag : StructuredItemsViewController2<ReorderableItemsView>.HeaderTag));
				Assert.Same(container, cell.PlatformView);

				content.Clip = null;
				content.Shadow = null;
				items.Clear();
				items.Add("Three");
				collection.LayoutIfNeeded();
				Assert.NotSame(container, platform.Superview);
				await AssertEventually(() =>
				{
					collection.LayoutIfNeeded();
					return collection.ViewWithTag(
						inFooter ? StructuredItemsViewController2<ReorderableItemsView>.FooterTag : StructuredItemsViewController2<ReorderableItemsView>.HeaderTag)
						is TemplatedCell2 currentCell && currentCell.PlatformView == platform;
				});
				Assert.NotNull(platform.Window);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GlobalSupplementaryTemplateRetainsRootGeometry(bool inFooter)
		{
			SetupBuilder();
			Label content = null;
			var items = new ObservableCollection<string> { "One" };
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 })
			};
			collectionView.SetValue(inFooter ? StructuredItemsView.FooterTemplateProperty : StructuredItemsView.HeaderTemplateProperty,
				new DataTemplate(() => content = new Label
				{
					Text = "Content",
					Margin = new Thickness(10, 12, 16, 14),
					WidthRequest = 80,
					HeightRequest = 30,
					HorizontalOptions = LayoutOptions.End,
					VerticalOptions = LayoutOptions.Start
				}));

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => content?.Handler?.PlatformView is UIView { Window: not null });
				for (var i = 0; i < 2; i++)
				{
					await AssertEventually(() => content.Frame.X >= 0 && content.Frame.Y >= 0);
					var cell = Assert.IsAssignableFrom<TemplatedCell2>(collection.ViewWithTag(
						inFooter ? StructuredItemsViewController2<ReorderableItemsView>.FooterTag : StructuredItemsViewController2<ReorderableItemsView>.HeaderTag));
					cell.SetNeedsLayout();
					cell.LayoutIfNeeded();
					var platform = Assert.IsAssignableFrom<UIView>(content.Handler.PlatformView);
					Assert.Equal(80, (double)platform.Frame.Width, 1);
					Assert.Equal(30, (double)platform.Frame.Height, 1);
					Assert.Equal(12, (double)platform.Frame.Y, 1);
					Assert.Equal((double)cell.Bounds.Width - 80 - 16, (double)platform.Frame.X, 1);
					items.Clear();
					items.Add("Two");
					collection.LayoutIfNeeded();
				}
			});
		}

		[Fact]
		public async Task GlobalSupplementaryFooterDoesNotCoverRowsAfterSourceReplacement()
		{
			SetupBuilder();
			var footer = new Button { Text = "Footer", HeightRequest = 50 };
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				Footer = footer,
				ItemsSource = new[] { "One" },
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 40 })
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => footer.Handler?.PlatformView is UIView { Window: not null });
				var platform = (UIView)footer.Handler.PlatformView;
				var oldCenter = platform.ConvertPointToView(new CGPoint(platform.Bounds.Width / 2, platform.Bounds.Height / 2), collection);
				Assert.True(collection.HitTest(oldCenter, null).IsDescendantOfView(platform));

				collectionView.ItemsSource = Enumerable.Range(1, 100).Select(i => $"Item {i}").ToArray();
				collection.LayoutIfNeeded();
				await AssertEventually(() => collection.NumberOfItemsInSection(0) == 100);
				await AssertEventually(() => collection.ContentSize.Height > collection.Bounds.Height
					&& collection.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Footer).Length == 0);
				var hit = collection.HitTest(oldCenter, null);
				Assert.NotNull(hit);
				Assert.False(hit.IsDescendantOfView(platform));

				collection.SetContentOffset(new CGPoint(0, Math.Max(0, collection.ContentSize.Height - collection.Bounds.Height)), false);
				collection.LayoutIfNeeded();
				await AssertEventually(() =>
				{
					collection.SetContentOffset(new CGPoint(0, Math.Max(0, collection.ContentSize.Height - collection.Bounds.Height)), false);
					collection.LayoutIfNeeded();
					var center = platform.ConvertPointToView(new CGPoint(platform.Bounds.Width / 2, platform.Bounds.Height / 2), collection);
					return collection.Bounds.Contains(center) && collection.HitTest(center, null)?.IsDescendantOfView(platform) == true;
				});
			});
		}
	}
}
