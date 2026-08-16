#nullable enable annotations

using System;
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
		const int LastItemIndex = 4;
		const string CenterItemChangedMessage = "CollectionView center item changed after portrait-to-landscape resize.";

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
		public async Task CenterItemRemainsSelectedAfterLandscapeResize()
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
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var centeredOnLastItem = new TaskCompletionSource();
			var centerIndexAfterResize = new TaskCompletionSource<int>();
			var resizeStarted = false;
			var currentPosition = 0;
			var collectionView = new CollectionView
			{
				HeightRequest = 260,
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" },
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				},
				ItemTemplate = new DataTemplate(() => new Label
				{
					WidthRequest = 240,
					HeightRequest = 240
				})
			};

			collectionView.Scrolled += (_, args) =>
			{
				if (args.CenterItemIndex < 0 || args.CenterItemIndex > LastItemIndex)
					return;

				if (resizeStarted)
					centerIndexAfterResize.TrySetResult(args.CenterItemIndex);

				if (args.CenterItemIndex != currentPosition)
				{
					currentPosition = args.CenterItemIndex;
					collectionView.ScrollTo(args.CenterItemIndex, position: ScrollToPosition.Center, animate: false);
				}

				if (args.CenterItemIndex == LastItemIndex)
					centeredOnLastItem.TrySetResult();
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				UICollectionView nativeCollectionView = handler.Controller.CollectionView;
				nativeCollectionView.Frame = new CGRect(0, 0, 390, 260);
				nativeCollectionView.SetNeedsLayout();
				nativeCollectionView.LayoutIfNeeded();

				collectionView.ScrollTo(LastItemIndex, position: ScrollToPosition.Center, animate: false);
				await centeredOnLastItem.Task;

				resizeStarted = true;
				nativeCollectionView.Frame = new CGRect(0, 0, 844, 260);
				nativeCollectionView.CollectionViewLayout.InvalidateLayout();
				nativeCollectionView.SetNeedsLayout();
				nativeCollectionView.LayoutIfNeeded();
				await nativeCollectionView.PerformBatchUpdatesAsync(() => { });

				var centerItemIndex = await centerIndexAfterResize.Task;
				Assert.True(centerItemIndex == LastItemIndex, CenterItemChangedMessage);
			});
		}
	}
}
