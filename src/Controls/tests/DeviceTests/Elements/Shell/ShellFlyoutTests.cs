
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

#if IOS
using UIKit;
#endif

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
#else
using Microsoft.Maui.Controls.Handlers;
#endif


namespace Microsoft.Maui.DeviceTests
{
	public partial class ShellTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task FlyoutContentUpdatesAfterChange()
		{
			var flyoutContent = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Content" }
			};

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			},
			async (shell, handler) =>
			{
				Assert.False(flyoutContent.IsLoaded);

				shell.FlyoutContent = flyoutContent;
				await OnLoadedAsync(flyoutContent);

				shell.FlyoutContent = null;
				await OnUnloadedAsync(flyoutContent);
			});
		}

#if !MACCATALYST
		[Fact]
		public async Task LogicalChildrenPropagateCorrectly()
		{
			var flyoutItemGrid = new Grid();
			var shellSectionGrid = new Grid();
			var shellContentGrid = new Grid();

			var flyoutItem = new FlyoutItem() { Items = { new ContentPage() } };
			var shellSection = new ShellSection() { Items = { new ContentPage() } };
			var shellContent = new ShellContent() { Content = new ContentPage() };

			// Validate the the bindingcontext of the flyout content only gets set to the Shell Part it came from
			flyoutItemGrid.BindingContextChanged += (_, _) =>
				Assert.True(flyoutItemGrid.BindingContext == flyoutItem || flyoutItem.BindingContext == null);

			shellSectionGrid.BindingContextChanged += (_, _) =>
				Assert.True(shellSectionGrid.BindingContext == shellSection || shellSection.BindingContext == null);

			shellContentGrid.BindingContextChanged += (_, _) =>
				Assert.True(shellContentGrid.BindingContext == shellContent || shellContent.BindingContext == null);

			Shell.SetItemTemplate(flyoutItem, new DataTemplate(() => flyoutItemGrid));
			Shell.SetItemTemplate(shellSection, new DataTemplate(() => shellSectionGrid));
			Shell.SetItemTemplate(shellContent, new DataTemplate(() => shellContentGrid));

			await RunShellTest(shell =>
			{
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.Items.Add(flyoutItem);
				shell.Items.Add(shellSection);
				shell.Items.Add(shellContent);
			},
			async (shell, handler) =>
			{
				await OnLoadedAsync(flyoutItemGrid);
				await OnLoadedAsync(shellSectionGrid);
				await OnLoadedAsync(shellContentGrid);

				Assert.Equal(flyoutItemGrid.Parent, flyoutItem);
				Assert.Equal(shellSectionGrid.Parent, shellSection);
				Assert.Equal(shellContentGrid.Parent, shellContent);

				Assert.Contains(flyoutItemGrid, (flyoutItem as IVisualTreeElement).GetVisualChildren());
				Assert.Contains(shellSectionGrid, (shellSection as IVisualTreeElement).GetVisualChildren());
				Assert.Contains(shellContentGrid, (shellContent as IVisualTreeElement).GetVisualChildren());
			});
		}

#if !WINDOWS
		[Theory]
		[ClassData(typeof(ShellFlyoutHeaderBehaviorTestCases))]
		public async Task FlyoutHeaderMinimumHeight(FlyoutHeaderBehavior behavior)
		{
			await RunShellTest(shell =>
			{
				var layout = new VerticalStackLayout()
				{
					new Label() { Text = "Flyout Header" }
				};

				shell.FlyoutHeader = layout;
				shell.FlyoutHeaderBehavior = behavior;
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);
				var flyoutFrame = GetFrameRelativeToFlyout(handler, shell.FlyoutHeader as IView);


				if (behavior == FlyoutHeaderBehavior.CollapseOnScroll)
				{
					// 56 was pulled from the ActionBar height on Android
					// and then just used across all three platforms for
					// the min height when using collapse on scroll
					AssertionExtensions.CloseEnough(56, flyoutFrame.Height);
				}
				else
				{
					Assert.True(flyoutFrame.Height < 56, $"Expected < 56 Actual: {flyoutFrame.Height}");
				}
			});
		}

		// This is mainly relevant for android because android will auto offset the content
		// based on the height of the flyout header.
		[Fact]
		public async Task FlyoutContentSetsCorrectBottomPaddingWhenMinHeightIsSetForFlyoutHeader()
		{
			var layout = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Header" }
			};

			await RunShellTest(shell =>
			{

				layout.MinimumHeightRequest = 30;
				shell.FlyoutHeader = layout;

				shell.FlyoutFooter = new Label() { Text = "Flyout Footer" };
				shell.FlyoutContent = new VerticalStackLayout() { new Label() { Text = "Flyout Content" } };
				shell.FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);

				var headerFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutHeader);
				var contentFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutContent);
				var footerFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutFooter);

				// validate footer position
				AssertionExtensions.CloseEnough(footerFrame.Y, headerFrame.Height + contentFrame.Height + GetSafeArea().Top);
			});
		}

		[Theory]
		[ClassData(typeof(ShellFlyoutHeaderBehaviorAndContentTestCases))]
		public async Task FlyoutHeaderContentAndFooterAllMeasureCorrectly(
			FlyoutHeaderBehavior behavior,
			string contentType,
			int? headerMarginTop,
			int? headerMarginBottom,
			int contentMarginTop,
			int contentMarginBottom)
		{
			// flyoutHeader.Margin.Top gets set to the SafeAreaPadding
			// so we have to account for that in the default setup
			var headerMargin = new Thickness(0, headerMarginTop ?? 0, 0, headerMarginBottom ?? 0);
			var contentMargin = new Thickness(0, contentMarginTop, 0, contentMarginBottom);
			var flyoutHeader = new Label() { Text = "Flyout Header", BackgroundColor = Colors.AliceBlue };

			// If margin top is null we don't set anything so safe area is added automatically
			if (headerMarginTop.HasValue)
			{
				flyoutHeader.Margin = headerMargin;
			}

			await RunShellTest(shell =>
			{
				shell.FlyoutHeader = flyoutHeader;
				shell.FlyoutFooter = new Label() { Text = "Flyout Footer" };
				shell.FlyoutHeaderBehavior = behavior;
				shell.FlyoutContent = ShellFlyoutHeaderBehaviorAndContentTestCases.GetFlyoutContentAction(contentType, contentMargin);
			},
			async (shell, handler) =>
			{
				if (!headerMarginTop.HasValue)
				{
					headerMargin.Top = GetSafeArea().Top;
				}

				await OpenFlyout(handler);

				var flyoutFrame = GetFlyoutFrame(handler);
				var headerFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutHeader);
				var contentFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutContent);
				var footerFrame = GetFrameRelativeToFlyout(handler, (IView)shell.FlyoutFooter);

				// validate header position
				AssertionExtensions.CloseEnough(0, headerFrame.X, message: "Header X");
				AssertionExtensions.CloseEnough(headerMargin.Top, headerFrame.Y, epsilon: 0.3, message: "Header Y");
				AssertionExtensions.CloseEnough(flyoutFrame.Width, headerFrame.Width, message: "Header Width");

				// validate content position
				var expectedContentY = headerMargin.Top + headerMargin.Bottom + contentMargin.Top;

#if IOS
				if (contentType != "ScrollView")
#endif
				{
					expectedContentY += headerFrame.Height;
				}
#if IOS
				else
				{
					var scrollViewContentInsetTop = ((UIScrollView)((IView)shell.FlyoutContent).Handler.PlatformView).ContentInset.Top;
					AssertionExtensions.CloseEnough(headerFrame.Height, scrollViewContentInsetTop, message: "Content ScrollView Inset Y");
				}
#endif

				AssertionExtensions.CloseEnough(0, contentFrame.X, message: "Content X");
				AssertionExtensions.CloseEnough(expectedContentY, contentFrame.Y, epsilon: 0.5, message: "Content Y");
				AssertionExtensions.CloseEnough(flyoutFrame.Width, contentFrame.Width, message: "Content Width");

				// validate footer position
				var expectedFooterY = expectedContentY + contentMargin.Bottom + contentFrame.Height;
				AssertionExtensions.CloseEnough(0, footerFrame.X, message: "Footer X");
				AssertionExtensions.CloseEnough(expectedFooterY, footerFrame.Y, epsilon: 0.6, message: "Footer Y");
				AssertionExtensions.CloseEnough(flyoutFrame.Width, footerFrame.Width, message: "Footer Width");

				//All three views should measure to the height of the flyout
				AssertionExtensions.CloseEnough(expectedFooterY + footerFrame.Height, flyoutFrame.Height, epsilon: 0.5, message: "Total Height");
			});
		}
#endif

#if ANDROID || IOS
		[Theory]
		[ClassData(typeof(ShellFlyoutHeaderScrollTestCases))]
		public async Task FlyoutHeaderScroll(FlyoutHeaderBehavior flyoutHeaderBehavior, string contentType)
		{
			var headerRequestedHeight = 250;
			var headerMinHeight = 100;

			await RunShellTest(shell =>
			{
				shell.FlyoutHeaderBehavior = flyoutHeaderBehavior;
				var layout = new VerticalStackLayout()
				{
					new Label()
					{
						Text = "Header Content"
					}
				};

				layout.HeightRequest = headerRequestedHeight;

				shell.FlyoutHeader = new ScrollView()
				{
					MinimumHeightRequest = headerMinHeight,
					Content = layout
				};

				ShellFlyoutHeaderScrollTestCases.SetFlyoutContent(contentType, shell);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);

				var initialBox = (shell.FlyoutHeader as IView).GetBoundingBox();

				AssertionExtensions.CloseEnough(headerRequestedHeight, initialBox.Height, 0.3);

				var bottomOffset = await ScrollFlyoutToBottom(handler);
				var scrolledBox = (shell.FlyoutHeader as IView).GetBoundingBox();

				if (flyoutHeaderBehavior == FlyoutHeaderBehavior.CollapseOnScroll)
				{
					AssertionExtensions.CloseEnough(headerMinHeight, scrolledBox.Height, 0.3, "Collapsed Header Height");
				}
				else
				{
					AssertionExtensions.CloseEnough(headerRequestedHeight, scrolledBox.Height, 0.3, "Header Height");

					if (flyoutHeaderBehavior == FlyoutHeaderBehavior.Scroll)
					{
						// scrolledBoy.Y is negative because the header is scrolled up
						var diff = scrolledBox.Y + headerRequestedHeight;
						var epsilon = 0.3;
						Assert.True(diff <= epsilon, $"Scrolled Header: position {scrolledBox.Y} is no enough to cover height ({scrolledBox.Height * -1}). Epsilon: {epsilon}");
					}
					else
					{
						AssertionExtensions.CloseEnough(GetSafeArea().Top, scrolledBox.Y, 0.3, "Header position");
					}
				}
			});
		}

		[Theory]
		[ClassData(typeof(ShellFlyoutTemplatePartsTestCases))]
		public async Task FlyoutCustomContentMargin(string testName)
		{

			Action<Shell, object> shellPart = ShellFlyoutTemplatePartsTestCases.GetTest(testName);
			var baselineContent = new VerticalStackLayout() { new Label() { Text = "Flyout Layout Part" } };
			Rect frameWithoutMargin = Rect.Zero;

			// determine the location of the templated content on the screen without a margin
			await RunShellTest(shell =>
			{
				shellPart(shell, baselineContent);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);
				frameWithoutMargin = GetFrameRelativeToFlyout(handler, baselineContent);
			});

			var content = new VerticalStackLayout() { new Label() { Text = "Flyout Layout Part" } };
			string partTesting = string.Empty;
			await RunShellTest(shell =>
			{
				content.Margin = new Thickness(20, 30, 0, 30);
				shellPart(shell, content);
			},
			async (shell, handler) =>
			{
				await OpenFlyout(handler);

				var frameWithMargin = GetFrameRelativeToFlyout(handler, content);
				var leftDiff = Math.Abs(Math.Abs(frameWithMargin.Left - (frameWithoutMargin.Left - baselineContent.Margin.Left)) - 20);
				double verticalDiff;

				// The Flyout Footer doesn't automatically offset from the top safe area so we don't need to account for it
				if (shell.FlyoutFooter != null)
					verticalDiff = Math.Abs(Math.Abs(frameWithMargin.Top - (frameWithoutMargin.Top)) - 30);
				else
					verticalDiff = Math.Abs(Math.Abs(frameWithMargin.Top - (frameWithoutMargin.Top - GetSafeArea().Top)) - 30);

				Assert.True(leftDiff < 0.2, $"{partTesting} Left Margin Incorrect. Frame w/ margin: {frameWithMargin}. Frame w/o margin : {frameWithoutMargin}");

				Assert.True(verticalDiff < 0.2, $"{partTesting} Top Margin Incorrect. Frame w/ margin: {frameWithMargin}. Frame w/o margin : {frameWithoutMargin}");
			});
		}

#endif

		Thickness GetSafeArea()
		{
#if IOS || MACCATALYST
			var insets = UIKit.UIApplication.SharedApplication.GetSafeAreaInsetsForWindow();
			return new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom);
#else
			return Thickness.Zero;
#endif
		}
#endif

		async Task RunShellTest(Action<Shell> action, Func<Shell, ShellHandler, Task> testAction)
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				action(shell);
				if (shell.Items.Count == 0)
					shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() } };
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await OnNavigatedToAsync(shell.CurrentPage);
				await testAction(shell, handler);
			});
		}
	}
}