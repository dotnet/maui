using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.AppCompat.View.Menu;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
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
					window.Handler?.DisconnectHandler();

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

#pragma warning disable XAOBS001 // Obsolete
				if (primaryCommand is MenuItemImpl menuItemImpl)
#pragma warning restore XAOBS001 // Obsolete
				{
					if (toolbarItem.Order != ToolbarItemOrder.Secondary)
					{
						Assert.True(menuItemImpl.RequiresActionButton(), "Secondary Menu Item `SetShowAsAction` not set correctly");
					}
					else
					{
						Assert.False(menuItemImpl.RequiresActionButton(), "Primary Menu Item `SetShowAsAction` not set correctly");
					}
				}
				else
				{
					throw new Exception($"MenuItem type is not MenuItemImpl. Please rework test to work with {primaryCommand}");
				}
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
			if (handler.VirtualView is VisualElement e)
			{
				handler = e.Window?.Handler ?? handler;
			}

			if (handler is IWindowHandler wh)
			{
				handler = wh.VirtualView.Content.Handler;
			}

			if (handler is Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer sr)
			{
				var shell = handler.VirtualView as Shell;
				var currentPage = shell.CurrentPage;

				if (currentPage?.Handler?.PlatformView is AView pagePlatformView)
				{
					var parentContainer = pagePlatformView.GetParentOfType<CoordinatorLayout>();
					var toolbar = parentContainer?.GetFirstChildOfType<MaterialToolbar>();
					return toolbar;
				}

				return null;
			}
			else
			{
				return GetPlatformToolbar(handler.MauiContext);
			}
		}

		protected string GetToolbarTitle(IElementHandler handler) =>
			GetPlatformToolbar(handler).Title;

		protected MaterialToolbar GetPlatformToolbar(IMauiContext mauiContext)
		{
			var navManager = mauiContext.GetNavigationRootManager();
			if (navManager?.RootView is null)
				return null;

			var appbarLayout =
				navManager.RootView.FindViewById<AViewGroup>(Resource.Id.navigationlayout_appbar);

			if (appbarLayout is null &&
				navManager.RootView is ContainerView cv &&
				cv.CurrentView is Shell shell)
			{
				if (shell.Handler is Controls.Platform.Compatibility.IShellContext sr)
				{
					var layout = sr.CurrentDrawerLayout;
					var content = layout?.GetFirstChildOfType<Controls.Platform.Compatibility.CustomFrameLayout>();
					appbarLayout = content?.GetFirstChildOfType<AppBarLayout>();
				}
			}

			var toolBar = appbarLayout?.GetFirstChildOfType<MaterialToolbar>();

			toolBar = toolBar ?? navManager.ToolbarElement?.Toolbar?.Handler?.PlatformView as
				MaterialToolbar;

			if (toolBar is null)
			{
				appbarLayout =
					(navManager?.RootView as AViewGroup)?.GetFirstChildOfType<AppBarLayout>();

				toolBar = appbarLayout?.GetFirstChildOfType<MaterialToolbar>();
			}

			return toolBar;
		}

		protected Size GetTitleViewExpectedSize(IElementHandler handler)
		{
			var context = handler.MauiContext.Context;
			var toolbar = GetPlatformToolbar(handler.MauiContext).GetFirstChildOfType<Microsoft.Maui.Controls.Toolbar.Container>();
			return new Size(context.FromPixels(toolbar.MeasuredWidth), context.FromPixels(toolbar.MeasuredHeight));
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

		protected void AssertTranslationMatches(global::Android.Views.View nativeView, double expectedTranslationX, double expectedTranslationY)
		{
			var context = nativeView?.Context ?? throw new InvalidOperationException("Context cannot be null.");

			var expectedXInPixels = context.ToPixels(expectedTranslationX);
			Assert.Equal(expectedXInPixels, nativeView.TranslationX, precision: 1);

			var expectedYInPixels = context.ToPixels(expectedTranslationY);
			Assert.Equal(expectedYInPixels, nativeView.TranslationY, precision: 1);
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
#pragma warning disable XAOBS001 // Obsolete
				handler.PlatformViewUnderTest.LayoutParameters = new FitWindowsFrameLayout.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);
#pragma warning restore XAOBS001 // Obsolete

				if (_window is Window window)
				{
					window.ModalNavigationManager.SetModalParentView(FakeActivityRootView);
				}

				return FakeActivityRootView;
			}

			public override void OnResume()
			{
				base.OnResume();

				bool isCreated = (_window as Window)?.IsCreated ?? false;
				bool isActivated = (_window as Window)?.IsActivated ?? false;

				if (!isCreated)
					_window.Created();

				if (!isActivated)
					_window.Activated();

				_taskCompletionSource.SetResult(true);
			}

			public override void OnDestroy()
			{
				base.OnDestroy();
				bool isDestroyed = (_window as Window)?.IsDestroyed ?? false;

				if (!isDestroyed)
					_window.Destroying();

				_finishedDestroying.SetResult(true);
			}
		}

#pragma warning disable XAOBS001 // Obsolete
		public class FakeActivityRootView : FitWindowsFrameLayout
#pragma warning restore XAOBS001 // Obsolete
		{
			public FakeActivityRootView(Context context) : base(context)
			{
				Id = AView.GenerateViewId();
			}
		}
	}
}
