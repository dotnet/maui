using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		[Theory]
		[InlineData(false, false, false, false, false)]
		[InlineData(true, false, false, false, false)]
		[InlineData(true, false, true, false, false)]
		[InlineData(false, true, false, false, false)]
		[InlineData(true, false, true, true, false)]
		[InlineData(false, true, true, true, false)]
		[InlineData(true, false, false, false, true)]
		[InlineData(false, true, true, false, true)]
		public async Task GlobalSupplementaryInputRetainsFocusWhenItemsChange(
			bool useSearchBar, bool inFooter, bool useTemplate, bool grouped, bool horizontal)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					RegisterCommonHandlers(handlers);
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<SearchBar, SearchBarHandler>();
				});
			});

			var items = new ObservableCollection<string> { "Item 0", "Item 1" };
			var groups = new ObservableCollection<ObservableCollection<string>> { items };
			InputView input = null;
			View CreateContent()
			{
				input = useSearchBar ? new SearchBar() : new Entry();
				return new VerticalStackLayout
				{
					WidthRequest = 180,
					Children = { new Label { Text = "Filter" }, input }
				};
			}

			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				IsGrouped = grouped,
				ItemsSource = grouped ? groups : items,
				ItemsLayout = new LinearItemsLayout(horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical),
				EmptyView = new Label { Text = "No results" },
				ItemTemplate = new DataTemplate(() => new Label { WidthRequest = 30, HeightRequest = 30 })
			};

			if (useTemplate)
				collectionView.SetValue(inFooter ? StructuredItemsView.FooterTemplateProperty : StructuredItemsView.HeaderTemplateProperty, new DataTemplate(CreateContent));
			else
				collectionView.SetValue(inFooter ? StructuredItemsView.FooterProperty : StructuredItemsView.HeaderProperty, CreateContent());

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				await AssertEventually(() => input?.Handler?.PlatformView is UIView { Window: not null });
				var originalInput = input;
				var platformInput = useSearchBar
					? Assert.IsAssignableFrom<UISearchBar>(input.Handler.PlatformView).SearchTextField
					: Assert.IsAssignableFrom<UITextField>(input.Handler.PlatformView);
				Assert.True(platformInput.BecomeFirstResponder());
				await AssertEventually(() => input.IsFocused);

				var focusLosses = 0;
				input.Unfocused += (_, _) => focusLosses++;
				input.TextChanged += (_, e) =>
				{
					var target = grouped ? new ObservableCollection<string>() : items;
					if (grouped)
						groups.Clear();
					else
						items.Clear();
					for (var i = 0; i < e.NewTextValue.Length % 3; i++)
						target.Add($"Item {i}");
					if (grouped)
						groups.Add(target);
				};

				for (var length = 1; length <= 4; length++)
				{
					input.Text = new string('a', length);
					handler.Controller.CollectionView.LayoutIfNeeded();
					await AssertEventually(() => handler.Controller.CollectionView.NumberOfItemsInSection(0) == length % 3);

					Assert.Same(originalInput, input);
					Assert.True(platformInput.IsFirstResponder);
					Assert.True(input.IsFocused);
					Assert.Equal(0, focusLosses);
				}
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task GlobalSupplementaryContentTracksLayoutAndReplacement(bool horizontal, bool rightToLeft)
		{
			SetupBuilder();
			var header = new Label { Text = "Header", WidthRequest = horizontal ? 80 : -1, HeightRequest = horizontal ? -1 : 60 };
			var footer = new Label { Text = "Footer", WidthRequest = horizontal ? 80 : -1, HeightRequest = horizontal ? -1 : 60 };
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
				Header = header,
				Footer = footer,
				ItemsLayout = new GridItemsLayout(2, horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical),
				ItemsSource = new[] { "One", "Two" },
				ItemTemplate = new DataTemplate(() => new Label { WidthRequest = 40, HeightRequest = 40 }),
				EmptyView = new Label { Text = "Empty" }
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => header.Handler?.PlatformView is UIView { Window: not null }
					&& footer.Handler?.PlatformView is UIView { Window: not null });

				void AssertContentMatchesSlot(View view, int tag)
				{
					var platformView = Assert.IsAssignableFrom<UIView>(view.Handler.PlatformView);
					var slot = collection.ViewWithTag(tag);
					Assert.NotNull(slot);
					var bounds = platformView.ConvertRectToView(platformView.Bounds, collection);
					Assert.Equal((double)slot.Frame.X, (double)bounds.X, 1);
					Assert.Equal((double)slot.Frame.Y, (double)bounds.Y, 1);
					Assert.Equal((double)slot.Frame.Width, (double)bounds.Width, 1);
					Assert.Equal((double)slot.Frame.Height, (double)bounds.Height, 1);
				}

				AssertContentMatchesSlot(header, StructuredItemsViewController2<ReorderableItemsView>.HeaderTag);
				AssertContentMatchesSlot(footer, StructuredItemsViewController2<ReorderableItemsView>.FooterTag);

				collection.SetContentOffset(horizontal ? new CGPoint(20, 0) : new CGPoint(0, 20), false);
				collection.LayoutIfNeeded();
				AssertContentMatchesSlot(header, StructuredItemsViewController2<ReorderableItemsView>.HeaderTag);

				collectionView.ItemsSource = Array.Empty<string>();
				collection.LayoutIfNeeded();
				AssertContentMatchesSlot(header, StructuredItemsViewController2<ReorderableItemsView>.HeaderTag);
				AssertContentMatchesSlot(footer, StructuredItemsViewController2<ReorderableItemsView>.FooterTag);

				collectionView.Header = "String header";
				collection.LayoutIfNeeded();
				await AssertEventually(() => header.Parent is null);

				var replacement = new Label { Text = "Replacement", WidthRequest = horizontal ? 100 : -1, HeightRequest = horizontal ? -1 : 80 };
				collectionView.Header = replacement;
				collection.LayoutIfNeeded();
				await AssertEventually(() => replacement.Handler?.PlatformView is UIView { Window: not null });

				collectionView.Header = null;
				collectionView.Footer = null;
				collection.LayoutIfNeeded();
				await AssertEventually(() => replacement.Parent is null && footer.Parent is null);
			});
		}

		[Fact]
		public async Task GlobalSupplementaryButtonsRemainHittableAfterReload()
		{
			SetupBuilder();
			var items = new ObservableCollection<string> { "One" };
			var header = new Button { Text = "Header", HeightRequest = 50 };
			var footer = new Button { Text = "Footer", HeightRequest = 50 };
			var collectionView = new CollectionView
			{
				WidthRequest = 300,
				HeightRequest = 300,
				Header = header,
				Footer = footer,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 })
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var collection = handler.Controller.CollectionView;
				await AssertEventually(() => header.Handler?.PlatformView is UIView { Window: not null }
					&& footer.Handler?.PlatformView is UIView { Window: not null });

				void AssertHittable(Button button)
				{
					var platformView = Assert.IsAssignableFrom<UIView>(button.Handler.PlatformView);
					var center = platformView.ConvertPointToView(
						new CGPoint(platformView.Bounds.Width / 2, platformView.Bounds.Height / 2), collection);
					var hit = collection.HitTest(center, null);
					Assert.NotNull(hit);
					Assert.True(hit.IsDescendantOfView(platformView));
				}

				AssertHittable(header);
				AssertHittable(footer);
				items.Clear();
				items.Add("Two");
				collection.LayoutIfNeeded();
				AssertHittable(header);
				AssertHittable(footer);
			});
		}
	}
}
