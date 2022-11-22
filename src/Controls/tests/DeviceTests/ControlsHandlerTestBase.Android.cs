using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using ALayoutInflater = Android.Views.LayoutInflater;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using ImportantForAccessibility = Android.Views.ImportantForAccessibility;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase
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

		Task SetupWindowForTests<THandler>(IWindow window, Func<Task> runTests, IMauiContext mauiContext = null)
			where THandler : class, IElementHandler
		{
			mauiContext ??= MauiContext;
			return InvokeOnMainThreadAsync(async () =>
			{
				AViewGroup rootView = MauiContext.Context.GetActivity().Window.DecorView as AViewGroup;
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
					await runTests.Invoke();
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

					// Unset the Support Action bar if the calling code has set the support action bar
					if (MauiContext.Context.GetActivity() is AppCompatActivity aca)
					{
						aca.SetSupportActionBar(null);
					}
				}
			});
		}

		public bool ToolbarItemsMatch(
			IElementHandler handler,
			params ToolbarItem[] toolbarItems)
		{
			var toolbar = GetPlatformToolbar(handler);
			var menu = toolbar.Menu;

			Assert.Equal(toolbarItems.Length, menu.Size());

			for (var i = 0; i < toolbarItems.Length; i++)
			{
				ToolbarItem toolbarItem = toolbarItems[i];
				var primaryCommand = menu.GetItem(i);
				Assert.Equal(toolbarItem.Text, $"{primaryCommand.TitleFormatted}");
			}

			return true;
		}

		protected AView GetTitleView(IElementHandler handler)
		{
			var toolbar = GetPlatformToolbar(handler);
			var container = toolbar?.GetFirstChildOfType<Controls.Toolbar.Container>();

			if (container != null && container.ChildCount > 0)
				return container.GetChildAt(0);

			return null;
		}

		protected MaterialToolbar GetPlatformToolbar(IElementHandler handler)
		{
			if (handler is IWindowHandler wh)
			{
				handler = wh.VirtualView.Content.Handler;
			}

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

		public bool IsNavigationBarVisible(IElementHandler handler) =>
			IsNavigationBarVisible(handler.MauiContext);

		public bool IsNavigationBarVisible(IMauiContext mauiContext)
		{
			return GetPlatformToolbar(mauiContext)?
					.LayoutParameters?.Height > 0;
		}

		protected bool IsBackButtonVisible(IElementHandler handler)
		{
			if (GetPlatformToolbar(handler)?.NavigationIcon is DrawerArrowDrawable dad)
				return dad.Progress == 1;

			return false;
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

			public FakeActivityRootView FakeActivityRootView { get; set; }

			public WindowTestFragment(IMauiContext mauiContext, IWindow window)
			{
				_mauiContext = mauiContext;
				_window = window;
			}

			public override AView OnCreateView(ALayoutInflater inflater, AViewGroup container, Bundle savedInstanceState)
			{
				ScopedMauiContext = _mauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);
				var handler = (WindowHandlerStub)_window.ToHandler(ScopedMauiContext);

				FakeActivityRootView = new FakeActivityRootView(ScopedMauiContext.Context);
				FakeActivityRootView.LayoutParameters = new LinearLayoutCompat.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);
				FakeActivityRootView.AddView(handler.PlatformViewUnderTest);
				handler.PlatformViewUnderTest.LayoutParameters = new FitWindowsFrameLayout.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);

				return FakeActivityRootView;
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

		public class FakeActivityRootView : FitWindowsFrameLayout
		{
			public FakeActivityRootView(Context context) : base(context)
			{
				Id = AView.GenerateViewId();
			}
		}
	}
}
