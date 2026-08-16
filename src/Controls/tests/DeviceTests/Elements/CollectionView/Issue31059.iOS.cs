#nullable enable annotations

using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public class Issue31059 : ControlsHandlerTestBase
	{
		const string IssueNumber = "31059";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task LastItemRemainsCenteredAfterPortraitToLandscapeResize()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			const int lastItemIndex = 4;
			int centeredIndex = -1;
			var collectionView = new CollectionView
			{
				ItemsSource = Enumerable.Range(1, 5).ToArray(),
				ItemTemplate = new DataTemplate(() => new Border
				{
					HeightRequest = 140,
					WidthRequest = 260,
					Content = new Label()
				}),
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				},
				SelectionMode = SelectionMode.Single
			};

			collectionView.Scrolled += (_, e) => centeredIndex = e.CenterItemIndex;

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var nativeCollectionView = handler.Controller.CollectionView;
				await nativeCollectionView.PerformBatchUpdatesAsync(() => { });

				nativeCollectionView.Frame = new CGRect(0, 0, 390, 844);
				nativeCollectionView.CollectionViewLayout.InvalidateLayout();
				nativeCollectionView.LayoutIfNeeded();

				collectionView.ScrollTo(lastItemIndex, position: ScrollToPosition.Center, animate: false);
				nativeCollectionView.LayoutIfNeeded();
				nativeCollectionView.Delegate.Scrolled(nativeCollectionView);
				Assert.Equal(lastItemIndex, centeredIndex);

				centeredIndex = -1;
				nativeCollectionView.Frame = new CGRect(0, 0, 844, 390);
				nativeCollectionView.CollectionViewLayout.InvalidateLayout();
				nativeCollectionView.LayoutIfNeeded();
				nativeCollectionView.Delegate.Scrolled(nativeCollectionView);

				Assert.True(
					centeredIndex == lastItemIndex,
					"CollectionView should keep item 5 centered after portrait-to-landscape resize.");
			});
		}
	}
}
