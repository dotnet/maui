using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.Controls.Platform.Compatibility.ShellFlyoutTemplatedContentRenderer;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AAnimation = Android.Views.Animations.Animation;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellRenderer)]
	public class ShellRendererTransitionTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task NavigatingFromModalToShellItemPreservesContentPosition(bool useTabBar) =>
			VerifyModalNavigationAsync(useTabBar: useTabBar);

		[Fact]
		public Task CustomItemRendererPreservesContentPositionAfterModalNavigation() =>
			VerifyModalNavigationAsync(useCustomRenderer: true);

		[Fact]
		public Task ResumingShellItemRequestsInsetsWithoutAnotherTransition() =>
			VerifyModalNavigationAsync(resumeAgain: true);

		[Fact]
		public Task CenteredEdgeToEdgeContentRemainsCenteredAfterModalNavigation() =>
			VerifyModalNavigationAsync(centerContent: true);

		[Fact]
		public async Task ResumingUnrelatedFragmentDoesNotRequestInsets()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler<Shell, TransitionProbeShellRenderer>();
			}));

			var page = new ContentPage { Content = new Label { Text = "Shell content" } };
			var shell = await InvokeOnMainThreadAsync(() => new Shell { Items = { page } });
			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page);
				var renderer = Assert.IsType<TransitionProbeShellRenderer>(shell.Handler);
				var current = renderer.LatestItemRenderer.Fragment;
				await AssertEventually(() => current.IsResumed);
				var manager = current.ParentFragmentManager;
				var container = Assert.IsAssignableFrom<ViewGroup>(current.View.Parent);
				using var unrelated = new InsetsRecordingFragment();
				try
				{
					manager.BeginTransaction()
						.Add(container.Id, unrelated)
						.SetMaxLifecycle(unrelated, AndroidX.Lifecycle.Lifecycle.State.Started)
						.CommitNow();
					var view = Assert.IsType<InsetsRecordingView>(unrelated.View);
					Assert.True(view.IsAttachedToWindow);
					Assert.False(unrelated.IsResumed);
					var requests = view.InsetRequestCount;

					// Observe direct requests synchronously, not window-wide inset redispatches.
					manager.BeginTransaction()
						.SetMaxLifecycle(unrelated, AndroidX.Lifecycle.Lifecycle.State.Resumed)
						.CommitNow();

					Assert.True(unrelated.IsResumed);
					Assert.Same(current, renderer.LatestItemRenderer.Fragment);
					Assert.Equal(requests, view.InsetRequestCount);
				}
				finally
				{
					manager.BeginTransaction().Remove(unrelated).CommitNow();
				}
			});
		}

		async Task VerifyModalNavigationAsync(
			bool useTabBar = false, bool useCustomRenderer = false, bool resumeAgain = false, bool centerContent = false)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler<Shell, TransitionProbeShellRenderer>();
			}));

			var label = new Label { Text = "First line\nSecond line", HorizontalOptions = LayoutOptions.Center };
			var contentLayout = new VerticalStackLayout
			{
				VerticalOptions = centerContent ? LayoutOptions.Center : LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Center,
				Children = { label }
			};
			if (centerContent)
				contentLayout.SafeAreaEdges = SafeAreaEdges.None;

			var targetPage = new ContentPage
			{
				Content = contentLayout
			};
			Shell.SetNavBarIsVisible(targetPage, false);
			ShellItem target = useTabBar ? new TabBar() : new ShellItem();
			target.Route = "TransitionProbeTarget";
			target.Items.Add(new ShellSection { Items = { new ShellContent { Content = targetPage } } });
			target.Items.Add(new ShellSection { Items = { new ShellContent { Content = new ContentPage() } } });
			target.Items.Add(new ShellSection { Items = { new ShellContent { Content = new ContentPage() } } });

			var homePage = new ContentPage { Content = new Label { Text = "Home" } };
			var shell = await InvokeOnMainThreadAsync(() =>
			{
				var shell = new Shell();
				shell.Items.Add(homePage);
				shell.Items.Add(target);
				return shell;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(homePage);
				var renderer = Assert.IsType<TransitionProbeShellRenderer>(shell.Handler);
				renderer.UseCustomItemRenderer = useCustomRenderer || resumeAgain;
				await shell.Navigation.PushModalAsync(new ContentPage { Content = new Label { Text = "Modal" } });
				await shell.GoToAsync("//TransitionProbeTarget");
				await AssertEventually(() => renderer.LatestItemRenderer?.Fragment.IsResumed == true);

				Assert.Same(target, renderer.LatestItemRenderer.ShellItem);
				Assert.Empty(homePage.Navigation.ModalStack);

				await OnLoadedAsync(label);
				var pageView = (AView)targetPage.Handler.PlatformView;
				var labelView = (AView)label.Handler.PlatformView;
				using var insets = ViewCompat.GetRootWindowInsets(pageView);
				Assert.NotNull(insets);
				var topInset = insets.GetInsets(WindowInsetsCompat.Type.StatusBars()).Top;
				Assert.True(topInset > 0);
				var fragment = renderer.LatestItemRenderer.Fragment;
				var root = fragment.View;

				if (resumeAgain)
				{
					await AssertEventually(() => root.ScaleX == 1 && root.ScaleY == 1 && root.Alpha == 1);
					var manager = fragment.ParentFragmentManager;
					manager.BeginTransaction()
						.SetMaxLifecycle(fragment, AndroidX.Lifecycle.Lifecycle.State.Started)
						.CommitNow();
					Assert.False(fragment.IsResumed);
					Assert.True(root.IsAttachedToWindow);
					var recordingView = Assert.IsType<InsetsRecordingView>(root);
					var requests = recordingView.InsetRequestCount;

					manager.BeginTransaction()
						.SetMaxLifecycle(fragment, AndroidX.Lifecycle.Lifecycle.State.Resumed)
						.CommitNow();
					Assert.True(fragment.IsResumed);
					Assert.Same(root, fragment.View);
					Assert.Equal(requests + 1, recordingView.InsetRequestCount);
				}

				var pageLocation = new int[2];
				var labelLocation = new int[2];
				var expectedY = 0;
				var positionMatches = await Wait(() =>
				{
					pageView.GetLocationOnScreen(pageLocation);
					labelView.GetLocationOnScreen(labelLocation);
					expectedY = centerContent
						? pageLocation[1] + (pageView.Height - labelView.Height) / 2
						: Math.Max(pageLocation[1], topInset);
					return pageView.Height > 0 && labelView.Height > 0 &&
						root.ScaleX == 1 && root.ScaleY == 1 && root.Alpha == 1 &&
						Math.Abs(labelLocation[1] - expectedY) <= 1;
				});
				Assert.True(positionMatches,
					$"Expected label Y={expectedY}, actual={labelLocation[1]}, page Y={pageLocation[1]}, " +
					$"page height={pageView.Height}, label height={labelView.Height}, status bar={topInset}, " +
					$"layout padding={((AView)contentLayout.Handler.PlatformView).PaddingTop}, page padding={pageView.PaddingTop}, " +
					$"root scale={root.ScaleY}, alpha={root.Alpha}");
			});
		}

		class TransitionProbeShellRenderer : ShellRenderer
		{
			public TransitionProbeShellRenderer()
			{
			}

			public IShellItemRenderer LatestItemRenderer { get; private set; }
			public bool UseCustomItemRenderer { get; set; }

			protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
			{
				return LatestItemRenderer = UseCustomItemRenderer
					? new CustomShellItemRenderer(this)
					: base.CreateShellItemRenderer(shellItem);
			}
		}

		class CustomShellItemRenderer : ShellItemRenderer
		{
			public CustomShellItemRenderer(IShellContext shellContext) : base(shellContext)
			{
			}

			public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, global::Android.OS.Bundle savedInstanceState)
			{
				var view = new InsetsRecordingView(inflater.Context);
				view.AddView(base.OnCreateView(inflater, container, savedInstanceState),
					new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
				return view;
			}
		}

		class InsetsRecordingFragment : AndroidX.Fragment.App.Fragment
		{
			public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, global::Android.OS.Bundle savedInstanceState) =>
				new InsetsRecordingView(inflater.Context);
		}

		class InsetsRecordingView : global::Android.Widget.FrameLayout
		{
			public InsetsRecordingView(global::Android.Content.Context context) : base(context)
			{
			}

			public int InsetRequestCount { get; private set; }

			public override void RequestApplyInsets()
			{
				InsetRequestCount++;
				base.RequestApplyInsets();
			}
		}
	}

	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		protected virtual MaterialToolbar GetShellToolbar(IElementHandler handler) =>
			GetPlatformToolbar(handler);

		[Theory]
		[InlineData(FlowDirection.LeftToRight, FlowDirection.RightToLeft, false, false)]
		[InlineData(FlowDirection.RightToLeft, FlowDirection.LeftToRight, false, false)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.LeftToRight, false, false)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.RightToLeft, false, false)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.LeftToRight, true, false)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.RightToLeft, true, false)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.LeftToRight, true, true)]
		[InlineData(FlowDirection.MatchParent, FlowDirection.RightToLeft, true, true)]
		public async Task ShellRouteSlidesSecondStepInsideModalNavigationPage(
			FlowDirection flowDirection, FlowDirection inheritedDirection, bool updateDirection, bool inheritFromShell)
		{
			SetupBuilder();
			const string flowRoute = "Issue21749Flow";
			const string nextStepRoute = "Issue21749NextStep";
			var shell = await CreateShellAsync(shell => shell.Items.Add(new ContentPage()));

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var window = shell.Window;
				var hadWindowFlowDirection = window.IsSet(Controls.Window.FlowDirectionProperty);
				var originalWindowFlowDirection = window.FlowDirection;
				var hadShellFlowDirection = shell.IsSet(VisualElement.FlowDirectionProperty);
				var originalShellFlowDirection = shell.FlowDirection;
				try
				{
					Routing.RegisterRoute(flowRoute, typeof(Issue21749ModalFlow));
					Routing.RegisterRoute(nextStepRoute, typeof(Issue21749FlowEndPage));
					if (inheritFromShell)
					{
						shell.FlowDirection = inheritedDirection;
						window.FlowDirection = FlowDirection.MatchParent;
					}
					else
						window.FlowDirection = inheritedDirection;

					await shell.GoToAsync(flowRoute, animate: true).WaitAsync(TimeSpan.FromSeconds(5));
					var flow = Assert.IsType<Issue21749ModalFlow>(Assert.Single(shell.Navigation.ModalStack));
					var startPage = flow.CurrentPage;
					if (flowDirection != FlowDirection.MatchParent)
						flow.FlowDirection = flowDirection;
					Assert.Equal(flowDirection, flow.FlowDirection);
					var manager = Assert.IsType<NavigationViewHandler>(flow.Handler).StackNavigationManager;
					manager.SetNavigationViewFragmentType(typeof(SlideProbeFragment));

					await VerifyPushAndPopAsync();
					if (updateDirection)
					{
						// Change direction while the same modal is open, rather than recreating it.
						inheritedDirection = inheritedDirection == FlowDirection.LeftToRight
							? FlowDirection.RightToLeft
							: FlowDirection.LeftToRight;
						if (inheritFromShell)
							shell.FlowDirection = inheritedDirection;
						else
							window.FlowDirection = inheritedDirection;
						await VerifyPushAndPopAsync();
					}

					async Task VerifyPushAndPopAsync()
					{
						if (inheritFromShell)
						{
							Assert.Equal(FlowDirection.MatchParent, window.FlowDirection);
							Assert.Equal(inheritedDirection, ((IWindow)window).FlowDirection);
						}
						var effectiveDirection = flowDirection == FlowDirection.MatchParent ? inheritedDirection : flowDirection;
						var nativeDirection = effectiveDirection == FlowDirection.RightToLeft ? LayoutDirection.Rtl : LayoutDirection.Ltr;
						await AssertEventually(() => ((AView)flow.Handler.PlatformView).LayoutDirection == nativeDirection,
							timeout: 5000, message: "The modal navigation view did not inherit the expected layout direction.");

						await shell.GoToAsync(nextStepRoute, animate: true).WaitAsync(TimeSpan.FromSeconds(5));

						Assert.Same(flow, Assert.Single(shell.Navigation.ModalStack));
						Assert.Collection(flow.Navigation.NavigationStack,
							page => Assert.Same(startPage, page),
							page => Assert.IsType<Issue21749FlowEndPage>(page));
						SlideProbeFragment fragment = null;
						await AssertEventually(() =>
							(fragment = manager.NavHost.ChildFragmentManager.PrimaryNavigationFragment as SlideProbeFragment)?.EnterAnimation != null &&
							fragment.View?.IsAttachedToWindow == true,
							timeout: 5000, message: "Step 2 did not create an enter animation on an attached fragment.");
						var enterAnimation = fragment.EnterAnimation;

						// Do not Reset/GetTransformation until Android itself has rendered the animation.
						// Otherwise a captured but never started animation could make this test pass.
						await AssertEventually(() => enterAnimation.HasStarted && enterAnimation.HasEnded,
							timeout: 5000, message: "Step 2's enter animation did not start and finish.");
						Assert.True(fragment.View.IsAttachedToWindow);

						await shell.GoToAsync("..", animate: true).WaitAsync(TimeSpan.FromSeconds(5));
						Assert.Same(startPage, flow.CurrentPage);
						Assert.Same(startPage, Assert.Single(flow.Navigation.NavigationStack));
						Assert.Same(flow, Assert.Single(shell.Navigation.ModalStack));
						await AssertEventually(() => fragment.ExitAnimation != null,
							timeout: 5000, message: "Step 2 did not create a back-navigation exit animation.");
						var exitAnimation = fragment.ExitAnimation;
						await AssertEventually(() => exitAnimation.HasStarted && exitAnimation.HasEnded,
							timeout: 5000, message: "Step 2's back-navigation exit animation did not start and finish.");
						await OnLoadedAsync(startPage).WaitAsync(TimeSpan.FromSeconds(5));

						var expectedOffsetSign = effectiveDirection == FlowDirection.RightToLeft ? -1 : 1;
						AssertSlideOffset(enterAnimation, expectedOffsetSign, "forward enter");
						AssertSlideOffset(exitAnimation, expectedOffsetSign, "back exit");
					}
				}
				finally
				{
					try
					{
						while (shell.Navigation.ModalStack.Count > 0)
							await shell.Navigation.PopModalAsync(animated: false).WaitAsync(TimeSpan.FromSeconds(5));
					}
					finally
					{
						if (hadShellFlowDirection)
							shell.FlowDirection = originalShellFlowDirection;
						else
							shell.ClearValue(VisualElement.FlowDirectionProperty);
						if (hadWindowFlowDirection)
							window.FlowDirection = originalWindowFlowDirection;
						else
							window.ClearValue(Controls.Window.FlowDirectionProperty);
						Routing.UnRegisterRoute(nextStepRoute);
						Routing.UnRegisterRoute(flowRoute);
					}
				}
			});
		}

		static void AssertSlideOffset(AAnimation animation, int expectedOffsetSign, string transition)
		{
			// Rendering has completed; sampling cannot interfere with Android's animation clock.
			const int size = 200;
			animation.Reset();
			animation.Initialize(size, size, size, size);
			animation.StartTime = 0;
			using var transformation = new Transformation();
			animation.GetTransformation(animation.ComputeDurationHint() / 2, transformation);
			var origin = new float[2];
			transformation.Matrix.MapPoints(origin);

			Assert.True(origin[0] * expectedOffsetSign > 0,
				$"Expected {transition} to have a {(expectedOffsetSign > 0 ? "positive" : "negative")} horizontal offset, but it was {origin[0]}.");
			Assert.InRange(transformation.Alpha, 0.01f, 0.99f);
		}

		public class SlideProbeFragment : NavigationViewFragment
		{
			internal AAnimation EnterAnimation { get; private set; }
			internal AAnimation ExitAnimation { get; private set; }

			public override AAnimation OnCreateAnimation(int transit, bool enter, int nextAnim)
			{
				var animation = base.OnCreateAnimation(transit, enter, nextAnim);
				if (enter)
					EnterAnimation = animation;
				else
					ExitAnimation = animation;

				return animation;
			}
		}

		class Issue21749ModalFlow : NavigationPage
		{
			public Issue21749ModalFlow() : base(new ContentPage { Title = "Step 1" })
			{
				Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);
			}
		}

		class Issue21749FlowEndPage : ContentPage
		{
			public Issue21749FlowEndPage()
			{
				Title = "Step 2";
			}
		}

		[Fact(DisplayName = "Collapsible SearchHandler Icon Uses ForegroundColor Set On Parent Shell Element")]
		public async Task CollapsibleSearchHandlerIconUsesForegroundColorSetOnParentShellElement()
		{
			SetupBuilder();

			var page = new ContentPage();
			Shell.SetSearchHandler(page, new SearchHandler { SearchBoxVisibility = SearchBoxVisibility.Collapsible });

			// The color is set on the TabBar, not on the page or on the Shell.
			var tabBar = new TabBar { Items = { new ShellContent { Content = page } } };
			Shell.SetBackgroundColor(tabBar, Colors.White);
			Shell.SetTitleColor(tabBar, Colors.Black);
			Shell.SetForegroundColor(tabBar, Colors.Red);

			var shell = await CreateShellAsync(shell => shell.Items.Add(tabBar));

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(page);

				var toolbar = GetShellToolbar(shell.Handler);
				Assert.Equal(1, toolbar.Menu.Size());

				using (var bitmap = RenderSearchIcon(toolbar))
					await bitmap.AssertContainsColor(Colors.Red);

				Shell.SetForegroundColor(tabBar, Colors.Lime);

				using (var bitmap = RenderSearchIcon(toolbar))
				{
					await bitmap.AssertContainsColor(Colors.Lime);
					await bitmap.AssertDoesNotContainColor(Colors.Red);
				}
			});

			static global::Android.Graphics.Bitmap RenderSearchIcon(MaterialToolbar toolbar)
			{
				// The collapsible SearchHandler is the only menu item.
				var icon = toolbar.Menu.GetItem(0).Icon;
				var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(icon.IntrinsicWidth, icon.IntrinsicHeight, global::Android.Graphics.Bitmap.Config.Argb8888);
				using var canvas = new global::Android.Graphics.Canvas(bitmap);
				icon.SetBounds(0, 0, canvas.Width, canvas.Height);
				icon.Draw(canvas);
				return bitmap;
			}
		}

		[Fact(DisplayName = "No crash going back using 'Shell.Current.GoToAsync(\"..\")'")]
		public async Task GoingBackUsingGoToAsyncMethod()
		{
			SetupBuilder();

			var page1 = new ContentPage();

			var page2Content = new Label { Text = "Test" };
			var page2 = new ContentPage { Content = page2Content };

			var pointerGestureRecognizer = new PointerGestureRecognizer();
			pointerGestureRecognizer.PointerPressed += (sender, args) =>
			{
				Console.WriteLine("Page Content pressed");
			};

			page2Content.GestureRecognizers.Add(pointerGestureRecognizer);

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							Route = "Item1",
							Content = page1
						},
						new ShellContent()
						{
							Route = "Item2",
							Content = page2
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await OnLoadedAsync(page1);
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.Navigation.PopAsync();

				await shell.GoToAsync("//Item1");
				await shell.GoToAsync("//Item2");
				await shell.GoToAsync("..");
			});
		}

		[Fact(DisplayName = "Overlapping GoToAsync(\"..\") calls each pop one page")]
		public async Task OverlappingRelativePopsEachPopAPage()
		{
			SetupBuilder();

			Routing.RegisterRoute("overlapPopFirst", typeof(QueryTrackingPage));
			Routing.RegisterRoute("overlapPopSecond", typeof(ContentPage));
			Routing.RegisterRoute("overlapPopThird", typeof(ContentPage));

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new ShellContent()
				{
					Route = "Item1",
					Content = new ContentPage()
				});
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await shell.GoToAsync($"overlapPopFirst?{nameof(QueryTrackingPage.TestValue)}=1");
				var firstPage = (QueryTrackingPage)shell.CurrentPage;
				await shell.GoToAsync("overlapPopSecond");
				await shell.GoToAsync("overlapPopThird");

				// Fire two relative pops without awaiting the first,
				// like a double tap on the hardware back button
				var pop1 = shell.GoToAsync("..");
				var pop2 = shell.GoToAsync("..");
				await Task.WhenAll(pop1, pop2);

				// Wait until the navigation stack stops changing before asserting
				int settledCount = -1;
				await AssertEventually(() =>
				{
					var count = shell.Navigation.NavigationStack.Count;
					if (count == settledCount)
						return true;

					settledCount = count;
					return false;
				}, timeout: 4000, interval: 500, message: "Navigation stack never settled");

				Assert.Equal(2, shell.Navigation.NavigationStack.Count);
				Assert.Same(firstPage, shell.CurrentPage);
				Assert.Equal("1", firstPage.TestValue);
			});
		}

		class QueryTrackingPage : ContentPage, IQueryAttributable
		{
			public string TestValue { get; private set; }

			public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query) =>
				TestValue = query.TryGetValue(nameof(TestValue), out var value) ? (string)value : null;
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task CanHideNavBarShadow(bool navBarHasShadow)
		{
			SetupBuilder();

			var contentPage = new ContentPage() { Title = "Flyout Item" };
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { contentPage } };

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Content"}
				};

				Shell.SetNavBarHasShadow(contentPage, navBarHasShadow);
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await Task.Delay(100);

				var platformToolbar = GetPlatformToolbar((IPlatformViewHandler)shell.Handler);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();

				if (navBarHasShadow)
					Assert.True(appBar.Elevation > 0);
				else
					Assert.True(appBar.Elevation == 0);
			});
		}

		[Fact(DisplayName = "Back button IconOverride keeps its aspect ratio")]
		public async Task BackButtonIconOverrideKeepsItsAspectRatio()
		{
			SetupBuilder();

			var icon = new FontImageSource { Glyph = "\u2039" };
			var secondPage = new ContentPage();
			Shell.SetBackButtonBehavior(secondPage, new BackButtonBehavior { IconOverride = icon });

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await shell.Navigation.PushAsync(secondPage);
				await AssertEventually(() => GetShellToolbar(shell.Handler)?.NavigationIcon is not null);

				var backButton = GetShellToolbar(shell.Handler).GetChildrenOfType<AppCompatImageButton>().First();
				using var backButtonBitmap = await backButton.ToBitmap(MauiContext);

				var iconDrawable = (await icon.GetPlatformImageAsync(MauiContext)).Value;
				iconDrawable.SetBounds(0, 0, iconDrawable.IntrinsicWidth, iconDrawable.IntrinsicHeight);
				using var iconBitmap = global::Android.Graphics.Bitmap.CreateBitmap(iconDrawable.IntrinsicWidth, iconDrawable.IntrinsicHeight, global::Android.Graphics.Bitmap.Config.Argb8888);
				using (var canvas = new global::Android.Graphics.Canvas(iconBitmap))
				{
					iconDrawable.Draw(canvas);
				}

				var expected = GetDrawnBounds(iconBitmap);
				var actual = GetDrawnBounds(backButtonBitmap);
				Assert.Equal((double)expected.Width() / expected.Height(), (double)actual.Width() / actual.Height(), 0.15);
			});
		}

		[Fact(DisplayName = "Back button IconOverride with an extreme aspect ratio is still drawn")]
		public async Task BackButtonIconOverrideWithExtremeAspectRatioIsDrawn()
		{
			SetupBuilder();

			byte[] iconBytes;
			using (var iconBitmap = global::Android.Graphics.Bitmap.CreateBitmap(1000, 1, global::Android.Graphics.Bitmap.Config.Argb8888))
			using (var stream = new System.IO.MemoryStream())
			{
				iconBitmap.EraseColor(global::Android.Graphics.Color.White);
				iconBitmap.Compress(global::Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
				iconBytes = stream.ToArray();
			}

			var secondPage = new ContentPage();
			Shell.SetBackButtonBehavior(secondPage, new BackButtonBehavior { IconOverride = ImageSource.FromStream(() => new System.IO.MemoryStream(iconBytes)) });

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new ContentPage();
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await shell.Navigation.PushAsync(secondPage);
				await AssertEventually(() => GetShellToolbar(shell.Handler)?.NavigationIcon is not null);

				var backButton = GetShellToolbar(shell.Handler).GetChildrenOfType<AppCompatImageButton>().First();
				using var backButtonBitmap = await backButton.ToBitmap(MauiContext);

				Assert.False(GetDrawnBounds(backButtonBitmap).IsEmpty);
			});
		}

		static global::Android.Graphics.Rect GetDrawnBounds(global::Android.Graphics.Bitmap bitmap)
		{
			var pixels = new int[bitmap.Width * bitmap.Height];
			bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);

			int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
			for (int i = 0; i < pixels.Length; i++)
			{
				if (global::Android.Graphics.Color.GetAlphaComponent(pixels[i]) > 127)
				{
					left = Math.Min(left, i % bitmap.Width);
					right = Math.Max(right, i % bitmap.Width);
					top = Math.Min(top, i / bitmap.Width);
					bottom = Math.Max(bottom, i / bitmap.Width);
				}
			}

			return new global::Android.Graphics.Rect(left, top, right + 1, bottom + 1);
		}

		[Fact(DisplayName = "Shell BackgroundColor preserves Material 3 AppBar lift-on-scroll styling")]
		public async Task BackgroundColorPreservesMaterial3AppBarLiftOnScrollStyling()
		{
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds || !RuntimeFeature.IsMaterial3Enabled)
				return;

			SetupBuilder();

			var backgroundColor = Colors.Orange;
			var contentPage = new ContentPage { Title = "Shell Content" };
			Shell.SetBackgroundColor(contentPage, backgroundColor);

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem { Items = { contentPage } };
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await OnLoadedAsync(contentPage);

				var platformToolbar = GetPlatformToolbar((IPlatformViewHandler)shell.Handler);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();
				Assert.NotNull(appBar);
				Assert.IsType<MaterialShapeDrawable>(appBar.Background);

				appBar.SetLifted(true);
				var liftedColor = new Color(
					backgroundColor.Red + ((1f - backgroundColor.Red) * 0.3f),
					backgroundColor.Green + ((1f - backgroundColor.Green) * 0.3f),
					backgroundColor.Blue + ((1f - backgroundColor.Blue) * 0.3f),
					backgroundColor.Alpha);

				await AssertEventually(() => GetAppBarBackgroundColor(appBar) == liftedColor.ToPlatform().ToArgb(),
					message: "Shell AppBar did not apply the Material 3 lifted background color after Shell.BackgroundColor was customized.");
			});
		}

		[Fact(DisplayName = "Hidden Shell navigation bar clears app bar inset padding")]
		public async Task HiddenShellNavigationBarClearsAppBarInsetPadding()
		{
			SetupBuilder();
			const int statusBarTopInset = 24;
			const int displayCutoutTopInset = 96;

			var contentPage = new ContentPage()
			{
				Title = "Test",
				Content = new Label { Text = "Content" }
			};

			var syntheticInsets = CreateTopCutoutInsets(statusBarTopInset, displayCutoutTopInset);

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { contentPage } };
			});

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async (handler) =>
			{
				await OnLoadedAsync(contentPage);
				await OnNavigatedToAsync(contentPage);

				MaterialToolbar platformToolbar = null;
				await AssertEventually(() =>
				{
					platformToolbar = GetPlatformToolbar(handler);
					return platformToolbar?.LayoutParameters?.Height > 0;
				},
					timeout: 2000,
					message: "Toolbar did not render before Shell.NavBarIsVisible was toggled.");

				Assert.NotNull(platformToolbar);
				var appBar = platformToolbar.Parent.GetParentOfType<AppBarLayout>();
				var rootCoordinator = appBar?.Parent.GetParentOfType<CoordinatorLayout>();
				Assert.NotNull(appBar);
				Assert.NotNull(rootCoordinator);
				var capturingListener = AttachCapturingWindowInsetsListener(rootCoordinator, platformToolbar);

				ViewCompat.DispatchApplyWindowInsets(rootCoordinator, syntheticInsets);

				await AssertEventually(() => capturingListener.InvocationCount > 0,
					timeout: 2000,
					message: "The Shell root did not receive the initial synthetic window insets dispatch.");

				await AssertEventually(() => appBar.PaddingTop == displayCutoutTopInset,
					timeout: 2000,
					message: "AppBar never received the synthetic display cutout top inset before the Shell navigation bar was hidden.");

				AssertTopInsets(capturingListener.LastAppliedInsets, expectedSystemBarsTop: 0, expectedDisplayCutoutTop: 0,
					message: "Visible Shell app bar should consume the synthetic top insets.");

				var visibleInsetsInvocationCount = capturingListener.InvocationCount;

				Shell.SetNavBarIsVisible(contentPage, false);

				await AssertEventually(() => platformToolbar.LayoutParameters?.Height == 0 && platformToolbar.Height == 0,
					timeout: 2000,
					message: "Toolbar did not fully collapse after Shell.NavBarIsVisible was set to false.");

				await AssertEventually(() => capturingListener.InvocationCount > visibleInsetsInvocationCount && appBar.PaddingTop == 0,
					timeout: 2000,
					message: "Shell.NavBarIsVisible did not trigger an inset redispatch that cleared the AppBar top padding.");

				// Re-dispatch the synthetic insets now that the nav bar is hidden so we can assert
				// with known values that the app bar no longer consumes the top insets.
				var hiddenInsetsInvocationCount = capturingListener.InvocationCount;
				ViewCompat.DispatchApplyWindowInsets(rootCoordinator, syntheticInsets);

				await AssertEventually(() => capturingListener.InvocationCount > hiddenInsetsInvocationCount,
					timeout: 2000,
					message: "Expected an additional inset dispatch after re-injecting synthetic insets post-nav-bar-hide.");

				AssertTopInsets(capturingListener.LastAppliedInsets,
					expectedSystemBarsTop: statusBarTopInset,
					expectedDisplayCutoutTop: displayCutoutTopInset,
					message: "Hidden Shell app bar should stop consuming the synthetic top insets.");
			});
		}

		protected virtual async Task CheckFlyoutState(IShellContext shellContext, bool desiredState)
		{
			var drawerLayout = GetDrawerLayout(shellContext);
			var flyout = drawerLayout.GetChildAt(1);

			if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
			{
				Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));
				return;
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			flyout.LayoutChange += OnLayoutChanged;

			try
			{
				await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
			}
			catch (TimeoutException)
			{

			}

			flyout.LayoutChange -= OnLayoutChanged;
			Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));

			return;

			void OnLayoutChanged(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
			{
				if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
				{
					taskCompletionSource.SetResult(true);
					flyout.LayoutChange -= OnLayoutChanged;
				}
			}
		}

		[Fact(DisplayName = "FlyoutItems Render When FlyoutBehavior Starts As Locked")]
		public async Task FlyoutItemsRendererWhenFlyoutBehaviorStartsAsLocked()
		{
			SetupBuilder();
			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await Task.Delay(100);
				var dl = GetDrawerLayout(shellContext);
				var flyoutContainer = GetFlyoutMenuReyclerView(shellContext);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}


		[Fact(DisplayName = "Shell with Flyout Disabled Doesn't Render Flyout")]
		public async Task ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;

			await CreateHandlerAndAddToWindow(shell, () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var dl = GetDrawerLayout(shellContext);
				Assert.Equal(1, dl.ChildCount);
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				Assert.Equal(2, dl.ChildCount);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "FooterTemplate Measures to Set Flyout Width When Flyout Locked")]
		public async Task FooterTemplateMeasuresToSetFlyoutWidth()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label(){ Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutBehavior = FlyoutBehavior.Locked;
				shell.FlyoutWidth = 20;
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await OnFrameSetToNotEmpty(footer);
				Assert.True(Math.Abs(20 - footer.Frame.Width) < 1);
				Assert.True(footer.Frame.Height > 0);
			});
		}

		[Fact(DisplayName = "Flyout Footer and Default Flyout Items Render")]
		public async Task FlyoutFooterRenderersWithDefaultFlyoutItems()
		{
			SetupBuilder();
			VerticalStackLayout footer = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutFooter = footer;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await Task.Delay(100);
				var dl = GetDrawerLayout(shellContext);
				await OpenFlyout(shellContext);

				var flyoutContainer = GetFlyoutMenuReyclerView(shellContext);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact]
		public async Task FlyoutItemsRenderWhenFlyoutHeaderIsSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Hello there"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await Task.Delay(100);
				var dl = GetDrawerLayout(shellContext);
				await OpenFlyout(shellContext);

				var flyoutContainer = GetFlyoutMenuReyclerView(shellContext);

				Assert.True(flyoutContainer.MeasuredWidth > 0);
				Assert.True(flyoutContainer.MeasuredHeight > 0);
			});
		}

		[Fact]
		public async Task FlyoutHeaderRendersCorrectSizeWithFlyoutContentSet()
		{
			SetupBuilder();
			VerticalStackLayout header = new VerticalStackLayout()
			{
				new Label() { Text = "Flyout Header"}
			};

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = header;

				shell.FlyoutContent = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Content"}
				};

				shell.FlyoutFooter = new VerticalStackLayout()
				{
					new Label(){ Text = "Flyout Footer"}
				};
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await Task.Delay(100);
				var headerPlatformView = header.ToPlatform();
				var appBar = headerPlatformView.GetParentOfType<AppBarLayout>();
				Assert.Equal(appBar.MeasuredHeight - appBar.PaddingTop, headerPlatformView.MeasuredHeight);
			});
		}

		[Fact]
		public async Task SwappingOutAndroidContextDoesntCrash()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem1", Items = { new ContentPage() }, Title = "Flyout Item" });
				shell.Items.Add(new FlyoutItem() { Route = "FlyoutItem2", Items = { new ContentPage() }, Title = "Flyout Item" });
			});

			var window = new Controls.Window(shell);
			var mauiContextStub1 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub1);

			var mauiContextStub2 = ContextStub.CreateNew(MauiContext);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);
				await Task.Delay(100);
				await shell.GoToAsync("//FlyoutItem1");
				await shell.GoToAsync("//FlyoutItem2");
			}, mauiContextStub2);
		}

		[Fact]
		public async Task ChangingBottomTabAttributesDoesntRecreateBottomTabs()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var menu = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>().Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var icon1 = menuItem1.Icon;
				var icon2 = menuItem2.Icon;
				var title1 = menuItem1.TitleFormatted;
				var title2 = menuItem2.TitleFormatted;

				shell.CurrentItem.Items[0].Title = "new Title 1";
				shell.CurrentItem.Items[0].Icon = "blue.png";

				shell.CurrentItem.Items[1].Title = "new Title 2";
				shell.CurrentItem.Items[1].Icon = "blue.png";

				// let the icon and title propagate
				await AssertEventually(() => menuItem1.Icon != icon1);

				menu = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>().Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menuItem1.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);
				menuItem2.Icon.AssertColorAtCenter(global::Android.Graphics.Color.Blue);

				Assert.NotEqual(icon1, menuItem1.Icon);
				Assert.NotEqual(icon2, menuItem2.Icon);
				Assert.NotEqual(title1, menuItem1.TitleFormatted);
				Assert.NotEqual(title2, menuItem2.TitleFormatted);
			});
		}

		[Fact]
		public async Task RemovingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var bottomView = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);

				shell.CurrentItem.Items.RemoveAt(2);

				// let the change propagate
				await AssertEventually(() => bottomView.Menu.Size() == 2);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));
			});
		}

		[Fact]
		public async Task AddingBottomTabDoesntRecreateMenu()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var bottomView = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem1 = menu.GetItem(0);
				var menuItem2 = menu.GetItem(1);
				var menuItem2Icon = menuItem2.Icon;

				shell.CurrentItem.Items.Insert(1, new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "green.png" });

				// let the change propagate
				await AssertEventually(() => bottomView.Menu.GetItem(1).Icon != menuItem2Icon);

				menu = bottomView.Menu;
				Assert.Equal(menuItem1, menu.GetItem(0));
				Assert.Equal(menuItem2, menu.GetItem(1));

				menu.GetItem(1).Icon.AssertColorAtCenter(global::Android.Graphics.Color.Green);
				menu.GetItem(2).Icon.AssertColorAtCenter(global::Android.Graphics.Color.Red);
			});
		}

		[Fact]
		public async Task ReusedBottomTabReappliesEnabledState()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1", Icon = "red.png" });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 2", Icon = "red.png", IsEnabled = false });
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 3", Icon = "red.png" });
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var bottomView = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;
				var menuItem2 = menu.GetItem(1);

				Assert.False(menuItem2.IsEnabled);

				// Remove an unrelated tab so SetupMenu re-runs and this position-stable
				// item goes through the reused-item branch — it must reapply the
				// disabled state on reuse, not silently reset it to enabled.
				shell.CurrentItem.Items.RemoveAt(2);

				// let the change propagate
				await AssertEventually(() => bottomView.Menu.Size() == 2);

				menu = bottomView.Menu;
				Assert.Equal(menuItem2, menu.GetItem(1));
				Assert.False(menuItem2.IsEnabled);
			});
		}

		[Fact]
		public async Task MoreOverflowItemIsReusedNotRecreated()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				for (int i = 1; i <= 7; i++)
				{
					shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = $"Tab {i}", Icon = "red.png" });
				}
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var bottomView = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>();
				var menu = bottomView.Menu;

				// 4 regular tabs + "More" overflow item covering the remaining 3
				Assert.Equal(5, menu.Size());
				var moreItem = menu.GetItem(4);
				Assert.Equal(BottomNavigationViewUtils.MoreTabId, moreItem.ItemId);

				// Remove one of the overflowed tabs while overflow is still active
				// (6 tabs remain, still over the 5-item max) — the "More" IMenuItem
				// must be reused, not recreated, since it's not structurally changing.
				shell.CurrentItem.Items.RemoveAt(6);

				// let the change propagate
				await AssertEventually(() => shell.CurrentItem.Items.Count == 6);

				menu = bottomView.Menu;
				Assert.Equal(5, menu.Size());
				Assert.Equal(moreItem, menu.GetItem(4));
				Assert.Equal(BottomNavigationViewUtils.MoreTabId, menu.GetItem(4).ItemId);
			});
		}

		//src/Compatibility/Core/tests/Android/ShellTests.cs
		[Fact(DisplayName = "Flyout Header Changes When Updated")]
		public async Task FlyoutHeaderReactsToChanges()
		{
			SetupBuilder();

			var initialHeader = new Label() { Text = "Hello" };
			var newHeader = new Label() { Text = "Hello Part 2" };

			var shell = await CreateShellAsync(shell =>
			{
				shell.CurrentItem = new FlyoutItem() { Items = { new ContentPage() }, Title = "Flyout Item" };
				shell.FlyoutHeader = initialHeader;
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var initialHeaderPlatformView = initialHeader.ToPlatform();
				Assert.NotNull(initialHeaderPlatformView);
				Assert.NotNull(initialHeader.Handler);

				shell.FlyoutHeader = newHeader;

				var newHeaderPlatformView = newHeader.ToPlatform();
				Assert.NotNull(newHeaderPlatformView);
				Assert.NotNull(newHeader.Handler);

				Assert.Null(initialHeader.Handler);

				await OpenFlyout(shellContext);

				var appBar = newHeaderPlatformView.GetParentOfType<AppBarLayout>();
				Assert.NotNull(appBar);
			});
		}

		//src/Compatibility/Core/tests/Android/ShellTests.cs
		[Fact(DisplayName = "Ensure Default Colors are White for BottomNavigationView")]
		public async Task ShellTabColorsDefaultToWhite()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new Tab() { Items = { new ContentPage() }, Title = "Tab 1" });
				SetupShellTabColorsTest(shell);
			});

			await CreateHandlerAndAddToWindow(shell, () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				var bottomNavigationView = GetDrawerLayout(shellContext).GetFirstChildOfType<BottomNavigationView>();
				Assert.NotNull(bottomNavigationView);

				var background = bottomNavigationView.Background;
				Assert.NotNull(background);

				if (background is ColorChangeRevealDrawable changeRevealDrawable)
				{
					Assert.Equal(global::Android.Graphics.Color.White, changeRevealDrawable.EndColor);
				}
			});
		}

		protected virtual void SetupShellTabColorsTest(Shell shell)
		{
		}

		[Fact(DisplayName = "ShellContentFragment.Destroy handles null _shellContext gracefully")]
		public async Task ShellContentFragmentDestroyHandlesNullShellContext()
		{
			SetupBuilder();

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						new ShellContent()
						{
							Route = "Item1",
							Content = new ContentPage { Title = "Page 1" }
						},
						new ShellContent()
						{
							Route = "Item2",
							Content = new ContentPage { Title = "Page 2" }
						},
					}
				});
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await OnLoadedAsync(shell.CurrentPage);
				await OnNavigatedToAsync(shell.CurrentPage);

				// Navigate to trigger fragment creation
				await shell.GoToAsync("//Item2");
				await OnNavigatedToAsync(shell.CurrentPage);

				// Test normal destruction - should work without issues
				await shell.GoToAsync("//Item1");
				await OnNavigatedToAsync(shell.CurrentPage);

				// Test null context scenario
				var exception = Record.Exception(() =>
				{
					// Create fragment with null context - this should not throw
					Page page = new ContentPage();
					var fragment = new ShellContentFragment((IShellContext)null, page);

					// Dispose the fragment which calls Destroy internally
					// This validates the null-conditional operators in Destroy method
					fragment.Dispose();
				});

				// Verify no exception was thrown - validates (_shellContext?.Shell as IShellController)?.RemoveAppearanceObserver(this);
				Assert.Null(exception);
			});
		}

		protected virtual AView GetFlyoutPlatformView(IShellContext shellContext)
		{
			var drawerLayout = GetDrawerLayout(shellContext);
			return drawerLayout.GetChildrenOfType<ShellFlyoutLayout>().First();
		}

		internal virtual Graphics.Rect GetFlyoutFrame(IShellContext shellContext)
		{
			var platformView = GetFlyoutPlatformView(shellContext);
			var context = platformView.Context;

			return new Graphics.Rect(0, 0,
				context.FromPixels(platformView.MeasuredWidth - (platformView.PaddingLeft + platformView.PaddingRight)),
				context.FromPixels(platformView.MeasuredHeight - (platformView.PaddingTop + platformView.PaddingBottom)));
		}

		internal virtual Graphics.Rect GetFrameRelativeToFlyout(IShellContext shellContext, IView view)
		{
			var platformView = (view.Handler as IPlatformViewHandler).PlatformView;
			return platformView.GetFrameRelativeTo(GetFlyoutPlatformView(shellContext));
		}

		protected virtual async Task OpenFlyout(IShellContext shellContext, TimeSpan? timeOut = null)
		{
			var flyoutView = GetFlyoutPlatformView(shellContext);
			var drawerLayout = GetDrawerLayout(shellContext);

			if (drawerLayout is ShellFlyoutRenderer sfr && !sfr.FlyoutFirstDrawPassFinished)
				await Task.Delay(10);

			var hamburger =
				GetPlatformToolbar((IPlatformViewHandler)shellContext).GetChildrenOfType<AppCompatImageButton>().FirstOrDefault() ??
				throw new InvalidOperationException("Unable to find Drawer Button");

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);

			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			drawerLayout.DrawerOpened += OnDrawerOpened;
			hamburger.PerformClick();

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			void OnDrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs e)
			{
				drawerLayout.DrawerOpened -= OnDrawerOpened;
				taskCompletionSource.SetResult(true);
			}
		}

		protected virtual async Task<double> ScrollFlyoutToBottom(IShellContext shellContext)
		{
			DrawerLayout dl = shellContext.CurrentDrawerLayout;
			var viewGroup = dl.GetChildAt(1) as ViewGroup;
			var scrollView = viewGroup?.GetChildAt(0);

			if (scrollView is RecyclerView rv)
			{
				return await ScrollRecyclerViewToBottom(rv);
			}
			else if (scrollView is MauiScrollView mauisv)
			{
				return await ScrollMauiScrollViewToBottom(mauisv);
			}
			else
			{
				throw new Exception("RecyclerView or MauiScrollView not found");
			}
		}

		async Task<double> ScrollRecyclerViewToBottom(RecyclerView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollToPosition(flyoutItems.GetAdapter().ItemCount - 1);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, AView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			// The appbar layout won't offset if you programmatically scroll the RecyclerView
			// I haven't found a way to match the exact behavior when you touch and scroll
			// I think we'd have to actually send touch events through adb

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		async Task<double> ScrollMauiScrollViewToBottom(MauiScrollView flyoutItems)
		{
			TaskCompletionSource<object> result = new TaskCompletionSource<object>();
			flyoutItems.ScrollChange += OnFlyoutItemsScrollChange;
			flyoutItems.ScrollTo(0, flyoutItems.Height);
			await result.Task.WaitAsync(TimeSpan.FromSeconds(2));
			await Task.Delay(10);

			void OnFlyoutItemsScrollChange(object sender, NestedScrollView.ScrollChangeEventArgs e)
			{
				flyoutItems.ScrollChange -= OnFlyoutItemsScrollChange;
				result.TrySetResult(true);
			}

			// The appbar layout won't offset if you programmatically scroll the RecyclerView
			// I haven't found a way to match the exact behavior when you touch and scroll
			// I think we'd have to actually send touch events through adb

			var coordinatorLayout = flyoutItems.Parent.GetParentOfType<CoordinatorLayout>();
			var appbarLayout = coordinatorLayout.GetFirstChildOfType<AppBarLayout>();
			var clLayoutParams = appbarLayout.LayoutParameters as CoordinatorLayout.LayoutParams;
			var behavior = clLayoutParams.Behavior as AppBarLayout.Behavior;
			var headerContainer = appbarLayout.GetFirstChildOfType<HeaderContainer>();

#pragma warning disable XAOBS001 // Obsolete
			var verticalOffset = flyoutItems.ComputeVerticalScrollOffset();
#pragma warning restore XAOBS001 // Obsolete
			behavior.OnNestedPreScroll(coordinatorLayout, appbarLayout, flyoutItems, 0, verticalOffset, new int[2], ViewCompat.TypeTouch);
			await Task.Delay(10);

			return verticalOffset;
		}

		protected virtual DrawerLayout GetDrawerLayout(IShellContext shellContext)
		{
			return shellContext.CurrentDrawerLayout;
		}

		static int GetAppBarBackgroundColor(AppBarLayout appBar)
		{
			var background = Assert.IsType<MaterialShapeDrawable>(appBar.Background);
			Assert.NotNull(background.FillColor);

			return background.FillColor.GetColorForState(
				appBar.GetDrawableState(),
				new global::Android.Graphics.Color(background.FillColor.DefaultColor));
		}

		protected virtual RecyclerView GetFlyoutMenuReyclerView(IShellContext shellContext)
		{
			DrawerLayout dl = shellContext.CurrentDrawerLayout;

			var flyout = dl.GetChildAt(0);
			RecyclerView flyoutContainer = null;

			var ViewGroup = dl.GetChildAt(1) as ViewGroup;
			var rc = ViewGroup.GetChildAt(0) as RecyclerView;

			if (dl.GetChildAt(1) is ViewGroup vg1 &&
				vg1.GetChildAt(0) is RecyclerView rvc)
			{
				flyoutContainer = rvc;
			}

			return flyoutContainer ?? throw new Exception("RecyclerView not found");
		}

		async Task TapToSelect(ContentPage page)
		{
			var shellContent = page.Parent as ShellContent;
			var shellSection = shellContent.Parent as ShellSection;
			var shellItem = shellSection.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;
			await OnNavigatedToAsync(shell.CurrentPage);

			if (shellItem != shell.CurrentItem)
				throw new NotImplementedException();

			if (shellSection != shell.CurrentItem.CurrentItem)
				throw new NotImplementedException();

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.GetParentOfType<ViewPager2>();

			pagerParent.CurrentItem = shellSection.Items.IndexOf(shellContent);
			await OnNavigatedToAsync(page);
		}
	}

	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TraitName, RendererHandlerVariant.AndroidShellRenderer)]
	public class ShellRendererTopTabTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ProgrammaticTopTabNavigationUpdatesToolbar()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler<Shell, ShellRenderer>();
			}));

			var firstPage = new ContentPage();
			var secondPage = new ContentPage();
			var otherPage = new ContentPage();
			var firstTitle = new Label { Text = "First" };
			var secondTitle = new Label { Text = "Second" };
			var updatedTitle = new Label { Text = "Updated First" };
			Shell.SetTitleView(firstPage, firstTitle);
			Shell.SetTitleView(secondPage, secondTitle);
			Shell.SetBackgroundColor(firstPage, Colors.Green);
			Shell.SetBackgroundColor(secondPage, Colors.Red);

			var shell = await InvokeOnMainThreadAsync(() => new Shell
			{
				Items =
				{
					new TabBar
					{
						Items =
						{
							new Tab
							{
								Route = "TabA",
								Items =
								{
									new ShellContent { Route = "First", Title = "First", Content = firstPage },
									new ShellContent { Route = "Second", Title = "Second", Content = secondPage }
								}
							},
							new Tab
							{
								Route = "TabB",
								Items = { new ShellContent { Route = "Other", Content = otherPage } }
							}
						}
					}
				}
			});

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				Assert.IsType<ShellRenderer>(shell.Handler);
				await OnNavigatedToAsync(firstPage);

				var pager = ((IPlatformViewHandler)firstPage.Handler).PlatformView.GetParentOfType<ViewPager2>();
				Assert.NotNull(pager);
				pager.CurrentItem = 1;
				await OnNavigatedToAsync(secondPage);
				await AssertTitleViewText(shell, "Second");

				await shell.GoToAsync("//TabB/Other");
				await OnNavigatedToAsync(otherPage);
				await shell.GoToAsync("//TabA/First");
				await OnNavigatedToAsync(firstPage);

				Assert.Same(firstPage, shell.CurrentPage);
				await AssertTitleViewText(shell, "First");

				Shell.SetTitleView(firstPage, updatedTitle);
				await AssertTitleViewText(shell, "Updated First");
			});
		}

		async Task AssertTitleViewText(Shell shell, string expected)
		{
			_ = await Wait(() => (GetTitleView(shell.Handler) as global::Android.Widget.TextView)?.Text == expected);
			Assert.Equal(expected, (GetTitleView(shell.Handler) as global::Android.Widget.TextView)?.Text);
		}
	}
}
