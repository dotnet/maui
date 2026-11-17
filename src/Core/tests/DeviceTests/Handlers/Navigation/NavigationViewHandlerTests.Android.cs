using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
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
	public partial class NavigationViewHandlerTests
	{
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
}