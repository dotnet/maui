#if IOS
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue37264 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SoftInputKeepsScrollViewContentAlignedEdgeToEdge()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
				});
			});

			var topLabel = new Label
			{
				HeightRequest = 52,
				HorizontalOptions = LayoutOptions.Fill,
				Text = "TOP EDGE LABEL"
			};
			var bottomLabel = new Label
			{
				HeightRequest = 52,
				HorizontalOptions = LayoutOptions.Fill,
				Text = "BOTTOM EDGE LABEL"
			};
			var content = new ArrangeTrackingStackLayout
			{
				topLabel,
				new Grid { HeightRequest = 90 },
				bottomLabel
			};
			var scrollView = new ScrollView
			{
				SafeAreaEdges = SafeAreaEdges.None,
				Content = content
			};
			var root = new Grid { scrollView };
			var page = new ContentPage { Content = root };

			await CreateHandlerAndAddToWindow<IWindowHandler>(page, async handler =>
			{
				var platformWindow = (UIWindow)handler.PlatformView;
				var restorePortrait = root.Height > root.Width;

				try
				{
					await SetOrientation(platformWindow, root, UIInterfaceOrientationMask.Landscape, () => root.Width > root.Height);
					await new Func<bool>(() =>
						root.Width > 0 &&
						topLabel.Width > 0 &&
						bottomLabel.Width > 0).AssertEventually();

					var arranged = content.WaitForNextArrange();
					scrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
					await arranged.WaitAsync(TimeSpan.FromSeconds(5));

					const double tolerance = 4;
					Assert.True(
						Math.Abs(root.Width - topLabel.Width) <= tolerance &&
						Math.Abs(root.Width - bottomLabel.Width) <= tolerance,
						$"SoftInput safe-area handling should keep ScrollView content aligned edge-to-edge. " +
						$"Page width: {root.Width:F1}; top width: {topLabel.Width:F1}; bottom width: {bottomLabel.Width:F1}.");
				}
				finally
				{
					if (restorePortrait)
						await SetOrientation(platformWindow, root, UIInterfaceOrientationMask.Portrait, () => root.Height > root.Width);
				}
			});
		}

		static async Task SetOrientation(UIWindow window, VisualElement root, UIInterfaceOrientationMask orientation, Func<bool> orientationReached)
		{
			if (orientationReached())
				return;

			var orientationChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			root.SizeChanged += OnSizeChanged;

			try
			{
				var windowScene = window.WindowScene ?? throw new InvalidOperationException("The test window is not attached to a window scene.");
				var preferences = new UIWindowSceneGeometryPreferencesIOS
				{
					InterfaceOrientations = orientation
				};

				windowScene.RequestGeometryUpdate(preferences, error =>
					orientationChanged.TrySetException(new InvalidOperationException($"Unable to change test orientation: {error.LocalizedDescription}")));
				OnSizeChanged(null, EventArgs.Empty);
				await orientationChanged.Task.WaitAsync(TimeSpan.FromSeconds(5));
			}
			finally
			{
				root.SizeChanged -= OnSizeChanged;
			}

			void OnSizeChanged(object sender, EventArgs e)
			{
				if (orientationReached())
					orientationChanged.TrySetResult(true);
			}
		}

		sealed class ArrangeTrackingStackLayout : VerticalStackLayout
		{
			TaskCompletionSource<bool> _nextArrange = new(TaskCreationOptions.RunContinuationsAsynchronously);

			public Task WaitForNextArrange()
			{
				_nextArrange = new(TaskCreationOptions.RunContinuationsAsynchronously);
				return _nextArrange.Task;
			}

			protected override Size ArrangeOverride(Rect bounds)
			{
				var result = base.ArrangeOverride(bounds);
				_nextArrange.TrySetResult(true);
				return result;
			}
		}
	}
}
#endif
