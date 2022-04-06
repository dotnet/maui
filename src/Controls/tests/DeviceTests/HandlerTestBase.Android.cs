using System;
using System.Threading.Tasks;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using ALayoutInflater = Android.Views.LayoutInflater;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using ImportantForAccessibility = Android.Views.ImportantForAccessibility;
using Google.Android.Material.AppBar;
using AndroidX.CoordinatorLayout.Widget;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ImportantForAccessibility == ImportantForAccessibility.Yes;


		protected bool GetExcludedWithChildren(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ImportantForAccessibility == ImportantForAccessibility.NoHideDescendants;

		public AView GetSemanticPlatformElement(IViewHandler viewHandler)
		{
			if (viewHandler.PlatformView is AndroidX.AppCompat.Widget.SearchView sv)
				return sv.FindViewById(Resource.Id.search_button)!;

			return (AView)viewHandler.PlatformView;
		}

		static Drawable _decorDrawable;
		Task RunWindowTest<THandler>(IWindow window, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				AViewGroup rootView = MauiContext.Context.GetActivity().Window.DecorView as AViewGroup;
				_decorDrawable ??= rootView.Background;
				var linearLayoutCompat = new LinearLayoutCompat(MauiContext.Context);

				var fragmentManager = MauiContext.GetFragmentManager();
				var viewFragment = new WindowTestFragment(MauiContext, window);

				try
				{
					linearLayoutCompat.Id = AView.GenerateViewId();
					rootView.AddView(linearLayoutCompat);

					fragmentManager
						.BeginTransaction()
						.Add(linearLayoutCompat.Id, viewFragment)
						.Commit();

					await viewFragment.FinishedLoading;

					if (window.Content is Shell shell)
						await OnLoadedAsync(shell.CurrentPage);

					if (typeof(THandler).IsAssignableFrom(window.Handler.GetType()))
						await action((THandler)window.Handler);
					else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
						await action((THandler)window.Content.Handler);
					else if (window.Content is ContentPage cp && typeof(THandler).IsAssignableFrom(cp.Content.Handler.GetType()))
						await action((THandler)cp.Content.Handler);
					else
						throw new Exception($"I can't work with {typeof(THandler)}");
				}
				finally
				{
					if (window.Handler != null)
					{
						window.Handler.DisconnectHandler();
					}

					fragmentManager
						.BeginTransaction()
						.Remove(viewFragment)
						.Commit();

					rootView.RemoveView(linearLayoutCompat);

					await linearLayoutCompat.OnUnloadedAsync();
					if (viewFragment.View != null)
						await viewFragment.View.OnUnloadedAsync();

					await viewFragment.FinishedDestroying;

					// This is mainly to remove changes to the decor view that shell imposes
					if (_decorDrawable != rootView.Background)
						rootView.Background = _decorDrawable;

					// Unset the Support Action bar if the calling code has set the support action bar
					if (MauiContext.Context.GetActivity() is AppCompatActivity aca)
					{
						aca.SetSupportActionBar(null);
					}

					// Ideally this wouldn't be needed but I haven't found the right platform
					// component I can key into for knowing when the world can move on
					await Task.Delay(1000);
					fragmentManager.ExecutePendingTransactions();
				}
			});
		}

		protected MaterialToolbar GetPlatformToolbar(IElementHandler handler)
		{
			if (handler is Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer sr)
			{
				var shell = handler.VirtualView as Shell;
				var currentPage = shell.CurrentPage;
				var pagePlatformView = currentPage.Handler.PlatformView as AView;
				var parentContainer = pagePlatformView.GetParentOfType<CoordinatorLayout>();
				var toolbar = parentContainer.GetFirstChildOfType<MaterialToolbar>();
				return toolbar;
			}
			else
			{
				return GetPlatformToolbar(handler.MauiContext);
			}
		}

		protected MaterialToolbar GetPlatformToolbar(IMauiContext mauiContext)
		{
			var navManager = mauiContext.GetNavigationRootManager();
			var appbarLayout =
				navManager?.RootView?.FindViewById<AViewGroup>(Resource.Id.navigationlayout_appbar);

			var toolBar = appbarLayout?.GetFirstChildOfType<MaterialToolbar>();

			toolBar = toolBar ?? navManager.ToolbarElement?.Toolbar?.Handler?.PlatformView as
				MaterialToolbar;

			if (toolBar == null)
			{
				appbarLayout =
					(navManager?.RootView as AViewGroup)?.GetFirstChildOfType<AppBarLayout>();

				toolBar = appbarLayout?.GetFirstChildOfType<MaterialToolbar>();
			}

			return toolBar;
		}

		protected bool IsBackButtonVisible(IElementHandler handler) =>
			GetPlatformToolbar(handler)?.NavigationIcon != null;

		bool IsBackButtonVisible(IMauiContext mauiContext)
		{
			return GetPlatformToolbar(mauiContext)?.NavigationIcon != null;
		}

		class WindowTestFragment : Fragment
		{
			TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
			TaskCompletionSource<bool> _finishedDestroying = new TaskCompletionSource<bool>();
			readonly IMauiContext _mauiContext;
			readonly IWindow _window;

			public IMauiContext ScopedMauiContext { get; set; }

			public Task FinishedLoading => _taskCompletionSource.Task;

			public Task FinishedDestroying => _taskCompletionSource.Task;

			public WindowTestFragment(IMauiContext mauiContext, IWindow window)
			{
				_mauiContext = mauiContext;
				_window = window;
			}

			public override AView OnCreateView(ALayoutInflater inflater, AViewGroup container, Bundle savedInstanceState)
			{
				ScopedMauiContext = _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);
				_ = _window.ToHandler(ScopedMauiContext);
				var rootView = ScopedMauiContext.GetNavigationRootManager().RootView;

				rootView.LayoutParameters = new LinearLayoutCompat.LayoutParams(500, 500);
				return rootView;
			}

			public override void OnResume()
			{
				base.OnResume();
				_taskCompletionSource.SetResult(true);
			}

			public override void OnDestroy()
			{
				base.OnDestroy();
				_finishedDestroying.SetResult(true);
			}
		}
	}
}
