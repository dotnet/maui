using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Java.Lang;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using ATextAlignment = Android.Views.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(NavigationAnimationSwitchTestCollection.Name)]
	public partial class NavigationViewHandlerTests
	{
		[Theory]
		[InlineData(LayoutDirection.Ltr, 1)]
		[InlineData(LayoutDirection.Rtl, -1)]
		public async Task NavigationAnimationResourcesSlideAndLegacySwitchPreservesPackagedAnimations(LayoutDirection direction, int expectedOffsetSign)
		{
			const string switchName = "Microsoft.Maui.Platform.Android.UseLegacyNavigationAnimations";
			var wasConfigured = AppContext.TryGetSwitch(switchName, out var previousValue);
			var root = new ButtonStub();
			var navigation = new NavigationViewStub { NavigationStack = new List<IView> { root } };

			try
			{
				AppContext.SetSwitch(switchName, false);
				await CreateNavigationViewHandlerAsync(navigation, handler =>
				{
					handler.PlatformView.LayoutDirection = direction;
					var manager = handler.StackNavigationManager;

					// Cover all four resources. The opt-out must match the APK's original resources,
					// including in RTL, rather than assuming AndroidX's merged resources are slides.
					// Shell's modal-route test separately verifies native rendering lifecycle.
					var context = handler.PlatformView.Context;
					foreach (var isPopping in new[] { false, true })
					{
						foreach (var enter in new[] { false, true })
						{
							var resourceName = isPopping
								? (enter ? "nav_default_pop_enter_anim" : "nav_default_pop_exit_anim")
								: (enter ? "nav_default_enter_anim" : "nav_default_exit_anim");
							var resourceId = context.Resources.GetIdentifier(resourceName, "anim", context.PackageName);
							Assert.True(resourceId != 0, $"Missing packaged animation {resourceName}.");
							using var expectedLegacy = AnimationUtils.LoadAnimation(context, resourceId);

							AppContext.SetSwitch(switchName, true);
							using var legacy = manager.OnCreateNavigationAnimation(context, isPopping, enter);
							Assert.NotNull(legacy);
							Assert.Equal(expectedLegacy.ComputeDurationHint(), legacy.ComputeDurationHint());
							foreach (var progress in new[] { 0.0, 0.25, 0.75, 1.0 })
								Assert.Equal(SampleNavigationAnimation(expectedLegacy, progress), SampleNavigationAnimation(legacy, progress));

							// Toggle on the same manager so a cached switch value cannot pass.
							AppContext.SetSwitch(switchName, false);
							using var modern = manager.OnCreateNavigationAnimation(context, isPopping, enter);
							Assert.NotNull(modern);
							var first = SampleNavigationAnimation(modern, 0.25);
							var last = SampleNavigationAnimation(modern, 0.75);
							var sign = expectedOffsetSign * (isPopping ? -1 : 1) * (enter ? 1 : -1);
							Assert.True(first.X * sign > 0 && last.X * sign > 0,
								$"{direction} {resourceName} must slide, not just fade.");
							Assert.True(enter ? first.Alpha < last.Alpha : first.Alpha > last.Alpha,
								$"{resourceName} must preserve the {(enter ? "enter" : "exit")} fade when mirrored.");
						}
					}
					return Task.CompletedTask;
				});
			}
			finally
			{
				AppContext.SetSwitch(switchName, wasConfigured && previousValue);
			}
		}

		static (float X, float Alpha) SampleNavigationAnimation(Animation animation, double progress)
		{
			const int size = 200;
			animation.Reset();
			animation.Initialize(size, size, size, size);
			animation.StartTime = 0;
			using var transformation = new Transformation();
			animation.GetTransformation((long)(animation.ComputeDurationHint() * progress), transformation);
			var origin = new float[2];
			transformation.Matrix.MapPoints(origin);
			return (origin[0], transformation.Alpha);
		}

		[Fact(DisplayName = "Popping Pages Doesn't Throw Java Exceptions")]
		public async Task PoppingPagesDoesntThrowJavaExceptions()
		{
			var page1 = new ButtonStub();
			var page2 = new ButtonStub();
			var page3 = new ButtonStub();
			NavigationViewStub navigationViewStub = new NavigationViewStub()
			{
				NavigationStack = new List<IView>()
				{
					page1, page2, page3
				}
			};

			await CreateNavigationViewHandlerAsync(navigationViewStub, async (handler) =>
			{
				// The exceptions are caught, but they still reach FirstChanceException handlers (and crash reporters)
				var exceptionMessages = new List<string>();
				AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;

				try
				{
					navigationViewStub.RequestNavigation(new NavigationRequest(new List<IView>() { page1 }, false));
					await navigationViewStub.OnNavigationFinished;
				}
				finally
				{
					AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
				}

				Assert.Empty(exceptionMessages);
				Assert.Equal(1, GetNativeNavigationStackCount(handler));

				void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
				{
					if (e.Exception is IllegalArgumentException)
						exceptionMessages.Add(e.Exception.Message);
				}
			});
		}

		int GetNativeNavigationStackCount(NavigationViewHandler navigationViewHandler)
		{
			int i = 0;
			var navController = navigationViewHandler.StackNavigationManager.NavHost.NavController;
			navController.IterateBackStack(_ => i++);

			return i;
		}

		Task CreateNavigationViewHandlerAsync(IStackNavigationView navigationView, Func<NavigationViewHandler, Task> action)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var context = MauiProgram.DefaultContext;

				var rootView = (context as AppCompatActivity).Window.DecorView as ViewGroup;
				var linearLayoutCompat = new LinearLayoutCompat(context);
				var fragmentManager = MauiContext.GetFragmentManager();
				var viewFragment = new NavViewFragment(MauiContext);

				try
				{
					linearLayoutCompat.Id = View.GenerateViewId();

					fragmentManager
						.BeginTransaction()

						.Add(linearLayoutCompat.Id, viewFragment)
						.Commit();

					rootView.AddView(linearLayoutCompat);
					await viewFragment.FinishedLoading;
					var handler = CreateHandler<NavigationViewHandler>(navigationView, viewFragment.ScopedMauiContext);

					if (navigationView is NavigationViewStub nvs && nvs.NavigationStack?.Count > 0)
					{
						navigationView.RequestNavigation(new NavigationRequest(nvs.NavigationStack, false));
						await nvs.OnNavigationFinished;
					}

					await action(handler);
				}
				finally
				{
					rootView.RemoveView(linearLayoutCompat);

					fragmentManager
						.BeginTransaction()
						.Remove(viewFragment)
						.Commit();
				}
			});
		}

		class NavViewFragment : Fragment
		{
			TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
			readonly IMauiContext _mauiContext;
			public IMauiContext ScopedMauiContext { get; set; }

			public Task FinishedLoading => _taskCompletionSource.Task;
			public NavViewFragment(IMauiContext mauiContext)
			{
				_mauiContext = mauiContext;
			}

			public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				ScopedMauiContext = _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);
				return ScopedMauiContext.GetNavigationRootManager().RootView;
			}

			public override void OnResume()
			{
				base.OnResume();
				_taskCompletionSource.SetResult(true);
			}
		}
	}

	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class NavigationAnimationSwitchTestCollection
	{
		public const string Name = "Navigation animation AppContext switch";
	}
}