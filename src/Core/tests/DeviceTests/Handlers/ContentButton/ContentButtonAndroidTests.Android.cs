using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Google.Android.Material.Card;
using Google.Android.Material.Shape;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using Xunit;
using AColor = Android.Graphics.Color;
using AInsets = AndroidX.Core.Graphics.Insets;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAndroidTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task ContentIsPresentationOnlyAndReplacementRestoresNativeState()
		{
			var first = new ButtonStub { Text = "First" };
			var second = new ButtonStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first };
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(1, handler.PlatformView.ChildCount);
				var container = Assert.IsAssignableFrom<ViewGroup>(handler.PlatformView.GetChildAt(0));
				var firstPlatformView = Assert.IsAssignableFrom<AView>(container.GetChildAt(0));
				var firstImportance = firstPlatformView.ImportantForAccessibility;
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, container.ImportantForAccessibility);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(1, handler.PlatformView.ChildCount);
				Assert.Same(container, handler.PlatformView.GetChildAt(0));
				var secondPlatformView = Assert.IsAssignableFrom<AView>(container.GetChildAt(0));
				var secondImportance = secondPlatformView.ImportantForAccessibility;
				Assert.Same(second.Handler?.PlatformView, secondPlatformView);
				Assert.Null(firstPlatformView.Parent);
				Assert.Equal(firstImportance, firstPlatformView.ImportantForAccessibility);
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, container.ImportantForAccessibility);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(0, handler.PlatformView.ChildCount);
				Assert.Null(secondPlatformView.Parent);
				Assert.Equal(secondImportance, secondPlatformView.ImportantForAccessibility);
				Assert.Equal(0, container.ChildCount);
			});
		}

		[Fact]
		public async Task ReplacingFocusedVirtualViewTransfersFocus()
		{
			var first = new ContentButtonStub();
			var second = new ContentButtonStub();

			await AttachAndRun(first, handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				Assert.True(first.IsFocused);

				handler.SetVirtualView(second);

				Assert.True(platformView.IsFocused);
				Assert.False(first.IsFocused);
				Assert.True(second.IsFocused);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task NativeMinimumWidthIsUsedToMeasureWrappingContent()
		{
			var content = new LabelStub
			{
				Text = new string('W', 80),
				Width = Dimension.Unset,
				Height = Dimension.Unset,
			};
			var button = new ContentButtonStub
			{
				Content = content,
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				MinimumWidth = 200,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				var context = platformView.Context!;
				var heightSpec = AView.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
				var narrowSpec = AView.MeasureSpec.MakeMeasureSpec((int)context.ToPixels(100), MeasureSpecMode.AtMost);
				var minimumSpec = AView.MeasureSpec.MakeMeasureSpec((int)context.ToPixels(200), MeasureSpecMode.AtMost);

				platformView.Measure(narrowSpec, heightSpec);
				var narrowSize = (platformView.MeasuredWidth, platformView.MeasuredHeight);
				platformView.ForceLayout();
				platformView.Measure(minimumSpec, heightSpec);

				Assert.Equal((int)context.ToPixels(200), narrowSize.MeasuredWidth);
				Assert.Equal(narrowSize, (platformView.MeasuredWidth, platformView.MeasuredHeight));
			});
		}

		[Fact]
		public async Task PostedThemeChangeIsIgnoredAfterDisconnect()
		{
			await AttachAndRun(new ContentButtonStub(), async handler =>
			{
				var platformView = handler.PlatformView;
				using var configuration = new Configuration(platformView.Resources!.Configuration!);
				var nightMode = configuration.UiMode & UiMode.NightMask;
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

				platformView.DispatchConfigurationChanged(configuration);
				((IElementHandler)handler).DisconnectHandler();
				Assert.Null(((IElementHandler)handler).PlatformView);
				Assert.Null(((IElementHandler)handler).VirtualView);

				var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				platformView.Post(() => completed.SetResult());
				await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			});
		}

		[Fact]
		public async Task PostedThemeChangeRefreshesConnectedNativeDefaultsAndStyling()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 3,
				CornerRadius = -1,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				using var themedCard = new MaterialCardView(platformView.Context!);
				var expectedRadius = themedCard.Radius;
				var expectedRipple = themedCard.RippleColor!.DefaultColor;
				using var replacementBackground = new ColorDrawable(AColor.Magenta);
				platformView.Background = replacementBackground;
				platformView.Radius = 99;
				platformView.UpdateRippleColor(Colors.Yellow);

				using var configuration = new Configuration(platformView.Resources!.Configuration!);
				var nightMode = configuration.UiMode & UiMode.NightMask;
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

				platformView.DispatchConfigurationChanged(configuration);
				await DrainPostedWork(platformView);

				Assert.True(platformView.TryGetMauiBackground(
					out _,
					out _,
					out var background,
					out var stroke,
					out var mask));
				Assert.Equal(expectedRadius, background.CornerRadius, 1);
				Assert.Equal(expectedRadius, stroke.CornerRadius, 1);
				Assert.Equal(expectedRadius, mask.CornerRadius, 1);
				Assert.Equal(expectedRipple, platformView.RippleColor!.DefaultColor);
				Assert.IsType<RippleDrawable>(platformView.Background);
			});
		}

		[Fact]
		public async Task ThemeChangeRefreshesDefaultsFromTheNewTheme()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var context = MauiContext.Context;
				using var dayConfiguration = new Configuration(context.Resources!.Configuration!);
				dayConfiguration.UiMode = (dayConfiguration.UiMode & ~UiMode.NightMask) | UiMode.NightNo;
				using var nightConfiguration = new Configuration(dayConfiguration);
				nightConfiguration.UiMode = (nightConfiguration.UiMode & ~UiMode.NightMask) | UiMode.NightYes;
				using var dayConfigurationContext = context.CreateConfigurationContext(dayConfiguration);
				using var nightConfigurationContext = context.CreateConfigurationContext(nightConfiguration);
				using var dayContext = new ContextThemeWrapper(dayConfigurationContext, Resource.Style.Maui_MainTheme_NoActionBar);
				using var nightContext = new ContextThemeWrapper(nightConfigurationContext, Resource.Style.Maui_Material3_Theme_NoActionBar);
				using var mutableContext = new MutableContextWrapper(dayContext);
				var button = new ContentButtonStub();
				var handler = new ContextContentButtonHandler(mutableContext);
				InitializeViewHandler(button, handler);
				using var host = new FrameLayout(context);
				host.AddView(handler.PlatformView);

				await host.AttachAndRun(async () =>
				{
					using var dayCard = new MaterialCardView(mutableContext);
					var dayBackground = dayCard.CardBackgroundColor!.DefaultColor;
					var dayBackgroundPixel = BackgroundCenterPixel(dayCard);
					var dayRadius = dayCard.Radius;
					mutableContext.SetBaseContext(nightContext);
					using var nightCard = new MaterialCardView(mutableContext);
					var nightBackground = nightCard.CardBackgroundColor!.DefaultColor;
					var nightBackgroundPixel = BackgroundCenterPixel(nightCard);
					var nightRadius = nightCard.Radius;
					Assert.NotEqual(dayBackground, nightBackground);
					Assert.NotEqual(dayBackgroundPixel, nightBackgroundPixel);
					Assert.NotEqual(dayRadius, nightRadius);

					handler.PlatformView.DispatchConfigurationChanged(nightConfiguration);
					await DrainPostedWork(handler.PlatformView);

					Assert.Equal(nightBackground, handler.PlatformView.CardBackgroundColor!.DefaultColor);
					Assert.Equal(nightBackgroundPixel, BackgroundCenterPixel(handler.PlatformView));
					Assert.Equal(nightRadius, handler.PlatformView.Radius);
				});
			});
		}

		[Fact]
		public async Task PostedThemeChangeRefreshesReplacementView()
		{
			await AttachAndRun(new ContentButtonStub(), async handler =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				using var themedCard = new MaterialCardView(platformView.Context!);
				var expectedRadius = themedCard.Radius;
				using var configuration = new Configuration(platformView.Resources!.Configuration!);
				var nightMode = configuration.UiMode & UiMode.NightMask;
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

				platformView.DispatchConfigurationChanged(configuration);
				var replacement = new ContentButtonStub();
				handler.SetVirtualView(replacement);
				platformView.Radius = 99;

				await DrainPostedWork(platformView);

				Assert.Same(replacement, ((IElementHandler)handler).VirtualView);
				Assert.Equal(expectedRadius, platformView.Radius);
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeRadiusSurvivesAttachmentAndThemeRefresh()
		{
			var button = new ContentButtonStub { CornerRadius = -1 };
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StyledContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					await DrainPostedWork(handler.PlatformView);
					Assert.Equal(StyledContentButtonHandler.NativeRadius, handler.PlatformView.Radius);

					using var configuration = new Configuration(handler.PlatformView.Resources!.Configuration!);
					var nightMode = configuration.UiMode & UiMode.NightMask;
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
						(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

					handler.PlatformView.DispatchConfigurationChanged(configuration);
					await DrainPostedWork(handler.PlatformView);
					Assert.Equal(StyledContentButtonHandler.NativeRadius, handler.PlatformView.Radius);
				});
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task HandlerProvidedCornerTreatmentsSurviveAttachmentAndThemeRefresh(bool differentCornerSizes)
		{
			var button = new ContentButtonStub { CornerRadius = -1 };
			var handler = new CutCornerContentButtonHandler(differentCornerSizes);
			await InvokeOnMainThreadAsync(() => InitializeViewHandler(button, handler));

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					void AssertNativeShape()
					{
						var shape = handler.PlatformView.ShapeAppearanceModel;
						Assert.IsType<CutCornerTreatment>(shape.TopLeftCorner);
						Assert.IsType<CutCornerTreatment>(shape.TopRightCorner);
						using var bounds = new global::Android.Graphics.RectF(0, 0, 200, 200);
						Assert.Equal(handler.TopLeftRadius,
							shape.TopLeftCornerSize.GetCornerSize(bounds));
						Assert.Equal(handler.TopRightRadius,
							shape.TopRightCornerSize.GetCornerSize(bounds));
					}

					await DrainPostedWork(handler.PlatformView);
					AssertNativeShape();

					using var configuration = new Configuration(handler.PlatformView.Resources!.Configuration!);
					var nightMode = configuration.UiMode & UiMode.NightMask;
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
						(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);
					handler.PlatformView.DispatchConfigurationChanged(configuration);
					await DrainPostedWork(handler.PlatformView);

					AssertNativeShape();
				});
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeBackgroundTintSurvivesThemeRefresh()
		{
			var button = new ContentButtonStub();
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, TintedBackgroundContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					await DrainPostedWork(handler.PlatformView);
					using var defaultCard = new MaterialCardView(handler.PlatformView.Context!);
					var expectedColor = handler.InitialColor;
					Assert.NotEqual(BackgroundCenterPixel(defaultCard), expectedColor);
					Assert.Equal(expectedColor, BackgroundCenterPixel(handler.PlatformView));

					using var configuration = new Configuration(handler.PlatformView.Resources!.Configuration!);
					var nightMode = configuration.UiMode & UiMode.NightMask;
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
						(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);
					handler.PlatformView.DispatchConfigurationChanged(configuration);
					await DrainPostedWork(handler.PlatformView);

					Assert.Equal(expectedColor, BackgroundCenterPixel(handler.PlatformView));
					button.Background = new SolidPaint(Colors.Blue);
					handler.UpdateValue(nameof(IView.Background));
					button.Background = null;
					handler.UpdateValue(nameof(IView.Background));
					Assert.Equal(expectedColor, BackgroundCenterPixel(handler.PlatformView));
				});
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeBackgroundAlphaSurvivesThemeRefresh()
		{
			var button = new ContentButtonStub();
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, TranslucentBackgroundContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					Assert.Equal(TranslucentBackgroundContentButtonHandler.Alpha, handler.PlatformView.Background!.Alpha);

					using var configuration = new Configuration(handler.PlatformView.Resources!.Configuration!);
					var nightMode = configuration.UiMode & UiMode.NightMask;
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
						(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);
					handler.PlatformView.DispatchConfigurationChanged(configuration);
					await DrainPostedWork(handler.PlatformView);

					Assert.Equal(TranslucentBackgroundContentButtonHandler.Alpha, handler.PlatformView.Background!.Alpha);
				});
			});
		}

		[Fact]
		public async Task StrokeColorMapperCustomizationSurvivesOtherStylingUpdates()
		{
			var button = new ContentButtonStub { StrokeColor = Colors.Blue, StrokeThickness = 1 };
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			mapper.AppendToMapping(nameof(IButtonStroke.StrokeColor), (handler, _) =>
				handler.PlatformView.SetStrokeColor(ColorStateList.ValueOf(AColor.Red)));
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				var platformView = handler.PlatformView;
				Assert.Equal(AColor.Red.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);

				button.StrokeThickness = 2;
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				button.CornerRadius = 6;
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));
				Assert.Equal(AColor.Red.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);
				Assert.Equal((int)platformView.Context!.ToPixels(2), platformView.StrokeWidth);

				button.Background = new SolidPaint(Colors.Green);
				handler.UpdateValue(nameof(IView.Background));
				button.Background = null;
				handler.UpdateValue(nameof(IView.Background));
				Assert.Equal(AColor.Red.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);

				button.StrokeThickness = -1;
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				Assert.Equal(AColor.Red.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);
			});
		}

		[Fact]
		public async Task ClearingStrokeThicknessRestoresHandlerProvidedWidth()
		{
			var button = new ContentButtonStub { StrokeColor = Colors.Blue, StrokeThickness = 3 };
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, WideStrokeContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				Assert.Equal((int)platformView.Context!.ToPixels(3), platformView.StrokeWidth);

				button.StrokeThickness = -1;
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));

				Assert.Equal(WideStrokeContentButtonHandler.Width, platformView.StrokeWidth);
				Assert.Equal(AColor.Blue.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);

				button.Background = new LinearGradientPaintStub(Colors.Red, Colors.Green);
				button.CornerRadius = 18;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var background, out var stroke, out var mask));
				var expectedRadius = (int)platformView.Context!.ToPixels(18) - WideStrokeContentButtonHandler.Width / 2;
				Assert.Equal(expectedRadius, background.CornerRadius, 1);
				Assert.Equal(expectedRadius, stroke.CornerRadius, 1);
				Assert.Equal(expectedRadius, mask.CornerRadius, 1);
			});
		}

		[Fact]
		public async Task DisableDuringPressCanDisconnectDuringRelease()
		{
			var pressed = 0;
			var released = 0;
			var button = new ContentButtonStub();

			await AttachAndRun(button, handler =>
			{
				button.Pressed = () =>
				{
					pressed++;
					button.IsEnabled = false;
					handler.UpdateValue(nameof(IView.IsEnabled));
				};
				button.Released = () =>
				{
					released++;
					((IElementHandler)handler).DisconnectHandler();
				};

				var exception = Record.Exception(() =>
					DispatchTouch(handler.PlatformView, MotionEventActions.Down, 1, 1));

				Assert.Null(exception);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);
				Assert.Null(((IElementHandler)handler).PlatformView);
				Assert.Null(((IElementHandler)handler).VirtualView);
				return Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RebindingReleasesOnlyThePreviouslyPressedView(bool replaceView)
		{
			var firstReleased = 0;
			var secondReleased = 0;
			var first = new ContentButtonStub { Released = () => firstReleased++ };
			var second = new ContentButtonStub { Released = () => secondReleased++ };

			await AttachAndRun(first, handler =>
			{
				var platformView = handler.PlatformView;
				DispatchTouch(platformView, MotionEventActions.Down);

				handler.SetVirtualView(replaceView ? second : first);
				Assert.Equal(replaceView ? 1 : 0, firstReleased);
				Assert.Equal(0, secondReleased);

				DispatchTouch(platformView, MotionEventActions.Cancel);
				Assert.Equal(1, firstReleased);
				Assert.Equal(0, secondReleased);

				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Up);
				Assert.Equal(replaceView ? 1 : 2, firstReleased);
				Assert.Equal(replaceView ? 1 : 0, secondReleased);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ReentrantReplacementKeepsNewestView()
		{
			var firstReleased = 0;
			var first = new ContentButtonStub();
			var second = new ContentButtonStub();
			var third = new ContentButtonStub();

			await AttachAndRun(first, handler =>
			{
				first.Released = () =>
				{
					firstReleased++;
					handler.SetVirtualView(third);
				};
				DispatchTouch(handler.PlatformView, MotionEventActions.Down);

				handler.SetVirtualView(second);

				Assert.Same(third, ((IElementHandler)handler).VirtualView);
				Assert.Equal(1, firstReleased);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ReplacementRefreshesSafeAreaListenerEligibility()
		{
			var content = new ButtonStub();
			var handler = await CreateHandlerAsync(new ContentButtonStub { Content = content });

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				using var parent = new InsetTestRecyclerView(MauiContext.Context);
				parent.AddView(platformView);
				MauiWindowInsetListener.RegisterParentForChildViews(parent);

				Assert.False(MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(platformView));

				handler.SetVirtualView(new ExplicitSafeAreaContentButtonStub { Content = content });
				Assert.True(MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(platformView));

				using var builder = new WindowInsetsCompat.Builder();
				using var insets = builder
					.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(40, 0, 0, 0))
					.Build();
				ViewCompat.DispatchApplyWindowInsets(platformView, insets);
				platformView.Layout(0, 0, 180, 90);

				Assert.Equal(MauiContext.Context.FromPixels(40), content.Frame.X, 1);
				parent.RemoveView(platformView);
			});
		}

		[Theory]
		[InlineData(false, 40, 0)]
		[InlineData(false, 0, 40)]
		[InlineData(true, 40, 0)]
		[InlineData(true, 0, 40)]
		[InlineData(true, 20, 40)]
		[InlineData(true, 0, 0)]
		public async Task SafeAreaInsetsStayOnPhysicalEdgeInRtl(bool rtl, int leftInset, int rightInset)
		{
			var content = new ButtonStub();
			var button = new ExplicitSafeAreaContentButtonStub
			{
				Content = content,
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
			};

			await AttachAndRun(button, handler =>
			{
				var platformView = handler.PlatformView;
				var width = platformView.Resources!.DisplayMetrics!.WidthPixels;
				var widthSpec = AView.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
				var heightSpec = AView.MeasureSpec.MakeMeasureSpec(100, MeasureSpecMode.Exactly);
				platformView.Measure(widthSpec, heightSpec);
				platformView.Layout(0, 0, width, 100);

				using var builder = new WindowInsetsCompat.Builder();
				using var insets = builder
					.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(leftInset, 0, rightInset, 0))
					.Build();
				((IHandleWindowInsets)platformView).HandleWindowInsets(platformView, insets);
				platformView.Measure(widthSpec, heightSpec);
				platformView.Layout(0, 0, width, 100);

				var nativeContent = Assert.IsAssignableFrom<AView>(content.Handler.PlatformView);
				Assert.Equal(leftInset, nativeContent.Left);
				Assert.Equal(width - rightInset, nativeContent.Right);
				return Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData(SafeAreaRegions.SoftInput, true, 80, 20, 0, 80)]
		[InlineData(SafeAreaRegions.All, true, 10, 40, 0, 10)]
		[InlineData(SafeAreaRegions.All, false, 10, 40, 0, 40)]
		[InlineData(SafeAreaRegions.SoftInput, false, 0, 40, 0, 0)]
		[InlineData(SafeAreaRegions.SoftInput, true, 80, 20, 30, 50)]
		[InlineData(SafeAreaRegions.SoftInput, true, 80, 20, 100, 0)]
		public async Task AnimatedKeyboardInsetsUpdateContentLayout(
			SafeAreaRegions bottomRegion, bool opening, int keyboardBottom, int containerBottom, int gapToScreenBottom, int expectedBottom)
		{
			var content = new ButtonStub();
			var button = new ExplicitSafeAreaContentButtonStub
			{
				Content = content,
				BottomRegion = bottomRegion,
			};

			await AttachAndRun(button, handler =>
			{
				var platformView = handler.PlatformView;
				var width = platformView.Resources!.DisplayMetrics!.WidthPixels;
				var widthSpec = AView.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
				var heightSpec = AView.MeasureSpec.MakeMeasureSpec(400, MeasureSpecMode.Exactly);
				platformView.Measure(widthSpec, heightSpec);
				platformView.Layout(0, 0, width, 400);
				var (_, screenHeight) = SafeAreaExtensionsTests.GetScreenSize(MauiContext.Context);
				var location = new int[2];
				platformView.GetLocationOnScreen(location);
				var top = screenHeight - location[1] - 400 - gapToScreenBottom;
				platformView.Layout(0, top, width, top + 400);
				platformView.GetLocationOnScreen(location);
				Assert.Equal(gapToScreenBottom, screenHeight - location[1] - platformView.Height);

				using var builder = new WindowInsetsCompat.Builder();
				using var initial = builder
					.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(20, 0, 30, 0))
					.Build();
				((IHandleWindowInsets)platformView).HandleWindowInsets(platformView, initial);

				using var animatedBuilder = new WindowInsetsCompat.Builder();
				using var animated = animatedBuilder
					.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(20, 0, 30, containerBottom))
					.SetInsets(WindowInsetsCompat.Type.Ime(), AInsets.Of(0, 0, 0, keyboardBottom))
					.SetVisible(WindowInsetsCompat.Type.Ime(), true)
					.Build();
				Assert.Equal(expectedBottom > 0, SafeAreaExtensions.ApplyAnimatedSoftInputInsetsPx(
					animated, button, MauiContext.Context, platformView, opening));
				platformView.Measure(widthSpec, heightSpec);
				platformView.Layout(0, top, width, top + 400);

				var nativeContent = Assert.IsAssignableFrom<AView>(content.Handler.PlatformView);
				Assert.Equal(20, nativeContent.Left);
				Assert.Equal(width - 30, nativeContent.Right);
				Assert.Equal(400 - expectedBottom, nativeContent.Bottom);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task DisabledPressedViewIsNotRetainedAfterReplacement()
		{
			var (reference, handler) = await DisablePressedButtonAndReplace();

			await AssertionExtensions.WaitForGC(reference);
			Assert.NotNull(((IElementHandler)handler).VirtualView);
			GC.KeepAlive(handler);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		async Task<(WeakReference Reference, ContentButtonHandler Handler)> DisablePressedButtonAndReplace()
		{
			var first = new ContentButtonStub();
			var handler = await CreateHandlerAsync(first);
			var reference = new WeakReference(first);

			await InvokeOnMainThreadAsync(() =>
			{
				DispatchTouch(handler.PlatformView, MotionEventActions.Down);
				first.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				handler.SetVirtualView(new ContentButtonStub());
			});

			return (reference, handler);
		}

		[Fact]
		public async Task PositionOnlyLayoutKeepsBackgroundWhileResizingRefreshesIt()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 12,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Layout(0, 0, 160, 80);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var background, out _, out _));
				var shape = platformView.ShapeAppearanceModel;

				platformView.Layout(20, 30, 180, 110);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var movedBackground, out _, out _));
				Assert.Same(background, movedBackground);

				platformView.Layout(20, 30, 200, 130);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var resizedBackground, out _, out _));
				Assert.NotSame(background, resizedBackground);
				Assert.Same(shape, platformView.ShapeAppearanceModel);
			});
		}

		[Fact]
		public async Task ResizeRerunsBackgroundMapperCustomization()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
			};
			var customBackground = new ColorDrawable(AColor.Magenta);
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			mapper.AppendToMapping(nameof(IView.Background), (handler, _) =>
				handler.PlatformView.Background = customBackground);
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				var platformView = handler.PlatformView;
				Assert.Same(customBackground, platformView.Background);

				platformView.Layout(0, 0, 160, 80);
				platformView.Layout(0, 0, 180, 90);

				Assert.Same(customBackground, platformView.Background);
			});
		}

		[Theory]
		[InlineData(nameof(IView.Background), false)]
		[InlineData(nameof(IView.Background), true)]
		[InlineData(nameof(IButtonStroke.StrokeColor), false)]
		[InlineData(nameof(IButtonStroke.StrokeColor), true)]
		[InlineData(nameof(IButtonStroke.StrokeThickness), false)]
		[InlineData(nameof(IButtonStroke.StrokeThickness), true)]
		public async Task BackgroundAndStrokeUpdatesPreserveCornerRadiusMapperCustomization(string property, bool gradient)
		{
			var button = new ContentButtonStub
			{
				Background = gradient ? new LinearGradientPaintStub(Colors.Red, Colors.Blue) : null,
				CornerRadius = 12,
			};
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			mapper.AppendToMapping(nameof(IButtonStroke.CornerRadius), (handler, _) =>
				handler.PlatformView.Radius = (float)handler.PlatformView.Context!.ToPixels(24));
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				var expectedRadius = (float)handler.PlatformView.Context!.ToPixels(24);
				Assert.Equal(expectedRadius, handler.PlatformView.Radius);

				button.Background = gradient
					? new LinearGradientPaintStub(Colors.Blue, Colors.Green)
					: new SolidPaint(Colors.Blue);
				button.StrokeColor = Colors.Red;
				button.StrokeThickness = 2;
				handler.UpdateValue(property);

				Assert.Equal(expectedRadius, handler.PlatformView.Radius);
			});
		}

		[Theory]
		[InlineData(nameof(IView.Background))]
		[InlineData(nameof(IButtonStroke.StrokeColor))]
		[InlineData(nameof(IButtonStroke.StrokeThickness))]
		public async Task BackgroundAndStrokeUpdatesRefreshGradientShape(string property)
		{
			var button = new ContentButtonStub
			{
				Background = property == nameof(IView.Background)
					? null
					: new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 24,
				StrokeColor = property == nameof(IButtonStroke.StrokeColor) ? null : Colors.Red,
				StrokeThickness = property == nameof(IButtonStroke.StrokeThickness) ? 0 : 10,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				if (property == nameof(IView.Background))
					button.Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue);
				else if (property == nameof(IButtonStroke.StrokeColor))
					button.StrokeColor = Colors.Red;
				else
					button.StrokeThickness = 10;

				handler.UpdateValue(property);

				var context = handler.PlatformView.Context!;
				var expectedRadius = (int)context.ToPixels(24) - (int)context.ToPixels(10) / 2;
				Assert.Equal(expectedRadius, handler.PlatformView.Radius);
			});
		}

		[Fact]
		public async Task DefaultStrokeColorWidthRefreshesGradientShape()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 24,
				StrokeColor = null,
				StrokeThickness = -1,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var context = handler.PlatformView.Context!;
				button.StrokeColor = Colors.Red;
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				Assert.Equal((int)context.ToPixels(24) - (int)context.ToPixels(1) / 2, handler.PlatformView.Radius);

				button.StrokeColor = null;
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				Assert.Equal((int)context.ToPixels(24), handler.PlatformView.Radius);
			});
		}

		[Fact]
		public async Task RemovingBackgroundRestoresNativeShapeRadius()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 24,
				StrokeColor = Colors.Red,
				StrokeThickness = 10,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var context = handler.PlatformView.Context!;
				Assert.Equal((int)context.ToPixels(24) - (int)context.ToPixels(10) / 2, handler.PlatformView.Radius);

				button.Background = null;
				handler.UpdateValue(nameof(IView.Background));
				Assert.Equal((int)context.ToPixels(24), handler.PlatformView.Radius);
			});
		}

		[Theory]
		[InlineData(nameof(IView.Background), false)]
		[InlineData(nameof(IView.Background), true)]
		[InlineData(nameof(IButtonStroke.StrokeColor), true)]
		[InlineData(nameof(IButtonStroke.StrokeThickness), true)]
		public async Task CornerRadiusMapperCanUpdateStylingWithoutRecursing(string property, bool setRadiusFirst)
		{
			var button = new ContentButtonStub { CornerRadius = 12 };
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			var testReentrancy = false;
			var updates = 0;
			mapper.AppendToMapping(nameof(IButtonStroke.CornerRadius), (handler, _) =>
			{
				if (!testReentrancy)
					return;

				if (++updates > 2)
					throw new InvalidOperationException("CornerRadius and styling mappings recursively update each other.");

				if (setRadiusFirst)
					handler.PlatformView.Radius = (float)handler.PlatformView.Context!.ToPixels(24);
				handler.UpdateValue(property);
				if (!setRadiusFirst)
					handler.PlatformView.Radius = (float)handler.PlatformView.Context!.ToPixels(24);
			});
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				testReentrancy = true;
				button.Background = new SolidPaint(Colors.Blue);
				button.StrokeColor = Colors.Red;
				button.StrokeThickness = 2;
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.Equal(1, updates);
				Assert.Equal((float)handler.PlatformView.Context!.ToPixels(24), handler.PlatformView.Radius);
			});
		}

		[Fact]
		public async Task ResizeDoesNotRebuildSolidBackground()
		{
			var button = new ContentButtonStub
			{
				Background = new SolidPaint(Colors.Red),
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Layout(0, 0, 160, 80);
				var background = platformView.Background;

				platformView.Layout(0, 0, 180, 90);

				Assert.Same(background, platformView.Background);
			});
		}

		[Theory]
		[InlineData(Keycode.Enter)]
		[InlineData(Keycode.DpadCenter)]
		[InlineData(Keycode.NumpadEnter)]
		[InlineData(Keycode.Space)]
		public async Task KeyboardActivationRaisesPressAndRelease(Keycode key)
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				using var down = new KeyEvent(KeyEventActions.Down, key);
				using var up = new KeyEvent(KeyEventActions.Up, key);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(down);
				Assert.Equal(1, pressed);
				Assert.Equal(0, released);
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);

				Assert.Equal(1, released);
				Assert.Equal(1, clicked);
			});
		}

		[Fact]
		public async Task NonConfirmKeysDoNotActivate()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());

				foreach (var key in new[] { Keycode.A, Keycode.Tab })
				{
					using var keyDown = new KeyEvent(KeyEventActions.Down, key);
					using var keyUp = new KeyEvent(KeyEventActions.Up, key);
					platformView.DispatchKeyEvent(keyDown);
					platformView.DispatchKeyEvent(keyUp);
				}

				await DrainPostedWork(platformView);
				Assert.Equal(0, pressed);
				Assert.Equal(0, released);
				Assert.Equal(0, clicked);

				using var down = new KeyEvent(KeyEventActions.Down, Keycode.Enter);
				using var up = new KeyEvent(KeyEventActions.Up, Keycode.Enter);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);

				Assert.Equal(1, pressed);
				Assert.Equal(1, released);
				Assert.Equal(1, clicked);
			});
		}

		[Fact]
		public async Task ContentReplacedWhileDetachingPreviousParentKeepsNewestContent()
		{
			var second = new LabelStub { Text = "Second" };
			var third = new LabelStub { Text = "Third" };
			var button = new ContentButtonStub { Content = new LabelStub { Text = "First" } };

			await AttachAndRun(button, handler =>
			{
				var platformView = handler.PlatformView;
				var host = new FrameLayout(platformView.Context!);
				var root = Assert.IsAssignableFrom<ViewGroup>(platformView.Parent);
				root.AddView(host);

				var secondView = second.ToPlatform(MauiContext);
				host.AddView(secondView);
				Assert.True(secondView.IsAttachedToWindow);

				var replaced = false;
				secondView.ViewDetachedFromWindow += (_, _) =>
				{
					if (replaced)
						return;

					// A native detach callback replaces the content while the previous one is being re-parented.
					replaced = true;
					button.Content = third;
					handler.UpdateValue(nameof(IContentView.Content));
				};

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.True(replaced);
				var container = Assert.IsAssignableFrom<ViewGroup>(Assert.Single(Children(platformView)));
				Assert.Same(third.Handler?.PlatformView, Assert.Single(Children(container)));
				Assert.Null(secondView.Parent);
				root.RemoveView(host);
				return Task.CompletedTask;
			});

			static AView[] Children(ViewGroup group) =>
				Enumerable.Range(0, group.ChildCount).Select(group.GetChildAt).OfType<AView>().ToArray();
		}

		[Fact]
		public async Task ClearingStrokeColorRestoresHandlerProvidedColor()
		{
			var button = new ContentButtonStub { StrokeColor = Colors.Blue, StrokeThickness = 3 };
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, RedStrokeContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				Assert.Equal(AColor.Blue.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);

				button.StrokeColor = null;
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));

				Assert.Equal(AColor.Red.ToArgb(), platformView.StrokeColorStateList!.DefaultColor);
				Assert.Equal((int)platformView.Context!.ToPixels(3), platformView.StrokeWidth);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task KeyboardActivationDoesNotClickReplacement(bool replaceOnPress)
		{
			var firstClicks = 0;
			var secondClicks = 0;
			var first = new ContentButtonStub { Clicked = () => firstClicks++ };
			var second = new ContentButtonStub { Clicked = () => secondClicks++ };

			await AttachAndRun(first, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				if (replaceOnPress)
					first.Pressed = () => handler.SetVirtualView(second);
				else
					first.Released = () => handler.SetVirtualView(second);

				using var down = new KeyEvent(KeyEventActions.Down, Keycode.Enter);
				using var up = new KeyEvent(KeyEventActions.Up, Keycode.Enter);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);

				Assert.Same(second, ((IElementHandler)handler).VirtualView);
				Assert.Equal(0, firstClicks);
				Assert.Equal(0, secondClicks);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);
				Assert.Equal(1, secondClicks);
			});
		}

		[Fact]
		public async Task DisablingDuringKeyboardPressCancelsActivation()
		{
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				button.Pressed = () =>
				{
					button.IsEnabled = false;
					handler.UpdateValue(nameof(IView.IsEnabled));
				};
				using var down = new KeyEvent(KeyEventActions.Down, Keycode.Enter);
				using var up = new KeyEvent(KeyEventActions.Up, Keycode.Enter);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);

				Assert.Equal(1, released);
				Assert.Equal(0, clicked);
				Assert.False(platformView.Pressed);
			});
		}

		[Fact]
		public async Task LosingFocusDuringKeyboardPressCancelsActivation()
		{
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				using var down = new KeyEvent(KeyEventActions.Down, Keycode.Enter);
				using var up = new KeyEvent(KeyEventActions.Up, Keycode.Enter);
				platformView.DispatchKeyEvent(down);
				platformView.ClearFocus();
				platformView.DispatchKeyEvent(up);
				await DrainPostedWork(platformView);

				Assert.Equal(1, released);
				Assert.Equal(0, clicked);
				Assert.False(platformView.Pressed);
			});
		}

		[Fact]
		public async Task CanceledKeyboardReleaseDoesNotActivate()
		{
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				platformView.FocusableInTouchMode = true;
				Assert.True(platformView.RequestFocus());
				using var down = new KeyEvent(KeyEventActions.Down, Keycode.Enter);
				using var up = new KeyEvent(KeyEventActions.Up, Keycode.Enter);
				using var canceled = KeyEvent.ChangeFlags(up, KeyEventFlags.Canceled);
				platformView.DispatchKeyEvent(down);
				platformView.DispatchKeyEvent(canceled);
				await DrainPostedWork(platformView);

				Assert.Equal(1, released);
				Assert.Equal(0, clicked);
				Assert.False(platformView.Pressed);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TouchDoesNotActivateReplacementView(bool replaceOnPress)
		{
			var firstClicks = 0;
			var secondClicks = 0;
			var second = new ContentButtonStub { Clicked = () => secondClicks++ };
			var first = new ContentButtonStub { Clicked = () => firstClicks++ };

			await AttachAndRun(first, async handler =>
			{
				var platformView = handler.PlatformView;
				if (replaceOnPress)
					first.Pressed = () => handler.SetVirtualView(second);
				else
					first.Released = () => handler.SetVirtualView(second);
				var downTime = SystemClock.UptimeMillis();
				DispatchTouch(platformView, MotionEventActions.Down, 1, 1, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, 1, 1, downTime);
				await Task.Delay(250);

				Assert.Same(second, ((IElementHandler)handler).VirtualView);
				Assert.Equal(0, firstClicks);
				Assert.Equal(0, secondClicks);

				downTime = SystemClock.UptimeMillis();
				DispatchTouch(platformView, MotionEventActions.Down, 1, 1, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, 1, 1, downTime);
				await Task.Delay(250);
				Assert.Equal(1, secondClicks);
			});
		}

		[Fact]
		public async Task QueuedReplacementDoesNotRedirectTouchClick()
		{
			var firstClicks = 0;
			var secondClicks = 0;
			var first = new ContentButtonStub { Clicked = () => firstClicks++ };
			var second = new ContentButtonStub { Clicked = () => secondClicks++ };

			await AttachAndRun(first, async handler =>
			{
				var platformView = handler.PlatformView;
				first.Released = () => platformView.Post(() => handler.SetVirtualView(second));

				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Up);

				Assert.Equal(1, firstClicks);
				Assert.Equal(0, secondClicks);
				await DrainPostedWork(platformView);
				Assert.Same(second, ((IElementHandler)handler).VirtualView);

				Assert.True(platformView.PerformClick());
				Assert.Equal(1, secondClicks);
			});
		}

		[Fact]
		public async Task QuickTapInScrollViewClearsNativePressedState()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformView = handler.PlatformView;
				using var scroll = new ScrollView(MauiContext.Context);
				using var content = new LinearLayout(MauiContext.Context) { Orientation = global::Android.Widget.Orientation.Vertical };
				using var filler = new AView(MauiContext.Context);
				content.AddView(platformView, new LinearLayout.LayoutParams(200, 100));
				content.AddView(filler, new LinearLayout.LayoutParams(200, 5000));
				scroll.AddView(content);

				await scroll.AttachAndRun(async () =>
				{
					await platformView.WaitForLayoutOrNonZeroSize();
					Assert.True(scroll.CanScrollVertically(1));
					Assert.True(scroll.ShouldDelayChildPressedState());
					var scrollLocation = new int[2];
					var buttonLocation = new int[2];
					scroll.GetLocationInWindow(scrollLocation);
					platformView.GetLocationInWindow(buttonLocation);
					var x = buttonLocation[0] - scrollLocation[0] + platformView.Width / 2f;
					var y = buttonLocation[1] - scrollLocation[1] + platformView.Height / 2f;
					var downTime = SystemClock.UptimeMillis();

					DispatchTouch(scroll, MotionEventActions.Down, x, y, downTime);
					Assert.Equal(1, pressed);
					Assert.False(platformView.Pressed, "The ScrollView should delay the native pressed state.");
					DispatchTouch(scroll, MotionEventActions.Up, x, y, downTime);
					await Task.Delay(ViewConfiguration.TapTimeout + 200);

					Assert.Equal(1, released);
					Assert.Equal(1, clicked);
					Assert.False(platformView.Pressed, "The native pressed state should stay clear after the tap timeout.");
				});
			});
		}

		[Fact]
		public async Task StatefulNativeBackgroundIsRestored()
		{
			var button = new ContentButtonStub();
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StatefulBackgroundHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				AssertColors();

				button.Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue);
				handler.UpdateValue(nameof(IView.Background));
				button.Background = null;
				handler.UpdateValue(nameof(IView.Background));
				AssertColors();

				void AssertColors()
				{
					var background = platformView.CardBackgroundColor!;
					Assert.Equal(AColor.Red, background.GetColorForState([global::Android.Resource.Attribute.StateEnabled], AColor.Transparent));
					Assert.Equal(AColor.Blue, background.GetColorForState([], AColor.Transparent));
				}
			});
		}

		[Fact]
		public async Task ReleaseOutsideBoundsCancelsClickWhileInsideReleaseActivates()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				var downTime = SystemClock.UptimeMillis();
				var insideX = platformView.Width - 1;
				var y = platformView.Height / 2f;
				DispatchTouch(platformView, MotionEventActions.Down, insideX, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Move, platformView.Width + 1, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, platformView.Width + 1, y, downTime);
				await Task.Delay(250);

				Assert.Equal(1, pressed);
				Assert.Equal(1, released);
				Assert.Equal(0, clicked);

				downTime = SystemClock.UptimeMillis();
				DispatchTouch(platformView, MotionEventActions.Down, platformView.Width / 2f, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, platformView.Width / 2f, y, downTime);
				await Task.Delay(250);

				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.Equal(1, clicked);
			});
		}

		[Fact]
		public async Task NativeCancelDisableDetachAndReattachReleasePressedState()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, handler =>
			{
				var platformView = handler.PlatformView;

				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Cancel);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);

				Assert.True(platformView.PerformClick());
				Assert.Equal(1, clicked);

				DispatchTouch(platformView, MotionEventActions.Down);
				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.False(platformView.Enabled);

				DispatchTouch(platformView, MotionEventActions.Down);
				Assert.Equal(2, pressed);
				platformView.PerformClick();
				Assert.Equal(1, clicked);

				button.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));
				DispatchTouch(platformView, MotionEventActions.Down);

				var parent = Assert.IsAssignableFrom<ViewGroup>(platformView.Parent);
				var layoutParameters = platformView.LayoutParameters;
				parent.RemoveView(platformView);
				Assert.Equal(3, pressed);
				Assert.Equal(3, released);

				parent.AddView(platformView, layoutParameters);
				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Up);
				Assert.Equal(4, pressed);
				Assert.Equal(4, released);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task AccessibilityExposesSingleNamedButtonAction()
		{
			var child = new ButtonStub { Text = "Presentation" };
			var button = new ContentButtonStub { Content = child };
			button.Semantics.Description = "Open details";
			await AttachAndRun(button, handler =>
			{
				using var info = handler.PlatformView.CreateAccessibilityNodeInfo();
				var compat = AccessibilityNodeInfoCompat.Wrap(info);

				Assert.Equal("android.widget.Button", info.ClassName);
				Assert.Equal("Open details", info.ContentDescription);
				Assert.True(info.Enabled);
				Assert.Contains(
					compat.ActionList,
					action => action.Id == AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id);

				Assert.Equal(1, handler.PlatformView.ChildCount);
				var platformChild = Assert.IsAssignableFrom<AView>(handler.PlatformView.GetChildAt(0));
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, platformChild.ImportantForAccessibility);

				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				using var disabledInfo = handler.PlatformView.CreateAccessibilityNodeInfo();
				Assert.False(disabledInfo.Enabled);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task GradientBackgroundStrokeAndRadiusCanReturnToNativeDefaults()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 3,
				CornerRadius = 18,
				Width = 160,
				Height = 80,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				Assert.True(platformView.TryGetMauiBackground(
					out var ripple,
					out _,
					out var background,
					out var stroke,
					out var mask));
				Assert.NotNull(ripple);

				var expectedRadius = button.GetStrokeProperties(platformView.Context!, false).Radius;
				Assert.Equal(expectedRadius, background.CornerRadius, 1);
				Assert.Equal(expectedRadius, stroke.CornerRadius, 1);
				Assert.Equal(expectedRadius, mask.CornerRadius, 1);

				button.Background = null;
				button.StrokeColor = null;
				button.StrokeThickness = -1;
				button.CornerRadius = -1;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.False(platformView.TryGetMauiBackground(out _, out _, out _, out _, out _));
			});
		}

		static Task DrainPostedWork(AView view)
		{
			var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			view.Post(() => completed.SetResult());
			return completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		static void DispatchTouch(AView view, MotionEventActions action, float x = 1, float y = 1, long? downTime = null)
		{
			var now = SystemClock.UptimeMillis();
			using var motionEvent = MotionEvent.Obtain(downTime ?? now, now, action, x, y, 0);
			view.DispatchTouchEvent(motionEvent);
		}

		sealed class StyledContentButtonHandler : ContentButtonHandler
		{
			internal const float NativeRadius = 12;

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Radius = NativeRadius;
				return button;
			}
		}

		sealed class CutCornerContentButtonHandler : ContentButtonHandler
		{
			readonly bool _differentCornerSizes;
			internal float TopLeftRadius { get; private set; }
			internal float TopRightRadius { get; private set; }

			internal CutCornerContentButtonHandler(bool differentCornerSizes) => _differentCornerSizes = differentCornerSizes;

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				TopLeftRadius = button.Radius;
				TopRightRadius = TopLeftRadius + (_differentCornerSizes ? 5 : 0);
				button.ShapeAppearanceModel = button.ShapeAppearanceModel.ToBuilder()
					.SetTopLeftCorner(CornerFamily.Cut, TopLeftRadius)
					.SetTopRightCorner(CornerFamily.Cut, TopRightRadius)
					.Build();
				return button;
			}
		}

		sealed class ContextContentButtonHandler : ContentButtonHandler
		{
			readonly Context _context;

			internal ContextContentButtonHandler(Context context) => _context = context;

			protected override MaterialCardView CreatePlatformView() =>
				new MauiContentButton(_context)
				{
					CardElevation = 0,
					CrossPlatformLayout = VirtualView,
					SoundEffectsEnabled = false,
				};
		}

		sealed class TintedBackgroundContentButtonHandler : ContentButtonHandler
		{
			internal int InitialColor { get; private set; }

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Background!.SetTint(AColor.Magenta);
				InitialColor = BackgroundCenterPixel(button);
				return button;
			}
		}

		sealed class TranslucentBackgroundContentButtonHandler : ContentButtonHandler
		{
			internal const int Alpha = 128;

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Background!.Alpha = Alpha;
				return button;
			}
		}

		sealed class WideStrokeContentButtonHandler : ContentButtonHandler
		{
			internal const int Width = 6;

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.StrokeWidth = Width;
				return button;
			}
		}

		sealed class RedStrokeContentButtonHandler : ContentButtonHandler
		{
			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.SetStrokeColor(ColorStateList.ValueOf(AColor.Red));
				return button;
			}
		}

		static int BackgroundCenterPixel(AView view)
		{
			using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(100, 100, global::Android.Graphics.Bitmap.Config.Argb8888!);
			using var canvas = new global::Android.Graphics.Canvas(bitmap!);
			var background = view.Background!;
			var bounds = background.Bounds!;
			var (left, top, right, bottom) = (bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			try
			{
				background.SetBounds(0, 0, 100, 100);
				background.Draw(canvas);
				return bitmap!.GetPixel(50, 50);
			}
			finally
			{
				background.SetBounds(left, top, right, bottom);
			}
		}

		sealed class StatefulBackgroundHandler : ContentButtonHandler
		{
			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				using var colors = new ColorStateList(
					[[global::Android.Resource.Attribute.StateEnabled], []],
					[AColor.Red.ToArgb(), AColor.Blue.ToArgb()]);
				button.CardBackgroundColor = colors;
				return button;
			}
		}

		sealed class InsetTestRecyclerView : FrameLayout, IMauiRecyclerView
		{
			public InsetTestRecyclerView(Context context) : base(context)
			{
			}
		}

		sealed class ExplicitSafeAreaContentButtonStub : ContentButtonStub, ISafeAreaView2
		{
			internal SafeAreaRegions BottomRegion { get; set; } = SafeAreaRegions.Container;

			public bool HasExplicitSafeAreaEdges => true;

			Thickness ISafeAreaView2.SafeAreaInsets { set { } }

			public SafeAreaRegions GetSafeAreaRegionsForEdge(int edge) =>
				edge == 3 ? BottomRegion : SafeAreaRegions.Container;
		}
	}
}
