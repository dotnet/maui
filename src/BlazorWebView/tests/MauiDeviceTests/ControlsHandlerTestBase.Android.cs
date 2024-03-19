using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
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
				//var fragmentManager = this.MauiContext.GetFragmentManager();
				var viewFragment = new WindowTestFragment(MauiContext, window);

				try
				{
					linearLayoutCompat.Id = AView.GenerateViewId();
					rootView.AddView(linearLayoutCompat);

					//fragmentManager
					//	.BeginTransaction()
					//	.Add(linearLayoutCompat.Id, viewFragment)
					//	.Commit();

					await viewFragment.FinishedLoading;
					await runTests.Invoke();
				}
				finally
				{
					window.Handler?.DisconnectHandler();

					//fragmentManager
					//	.BeginTransaction()
					//	.Remove(viewFragment)
					//	.Commit();

					rootView.RemoveView(linearLayoutCompat);

					await viewFragment.FinishedDestroying;

					// Unset the Support Action bar if the calling code has set the support action bar
					if (MauiContext.Context.GetActivity() is AppCompatActivity aca)
					{
						aca.SetSupportActionBar(null);
					}
				}
			});
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
			public static IMauiContext MakeScoped(IMauiContext mauiContext,
					LayoutInflater layoutInflater = null,
					FragmentManager fragmentManager = null,
					Android.Content.Context context = null,
					bool registerNewNavigationRoot = false)
			{
				var scopedContext = new MauiContext(mauiContext.Services);


				if (registerNewNavigationRoot)
				{
					if (fragmentManager == null)
						throw new InvalidOperationException("If you're creating a new Navigation Root you need to use a new Fragment Manager");

					//scopedContext.AddSpecific(new NavigationRootManager(scopedContext));
				}

				return scopedContext;
			}
			public override AView OnCreateView(ALayoutInflater inflater, AViewGroup container, Bundle savedInstanceState)
			{
				ScopedMauiContext = MakeScoped(_mauiContext, layoutInflater: inflater, fragmentManager: ChildFragmentManager, registerNewNavigationRoot: true);
				var handler = _window.ToHandler(ScopedMauiContext);

				FakeActivityRootView = new FakeActivityRootView(ScopedMauiContext.Context);
				FakeActivityRootView.LayoutParameters = new LinearLayoutCompat.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);
				//FakeActivityRootView.AddView((AView)(handler.PlatformViewUnderTest));
				//handler.PlatformViewUnderTest.LayoutParameters = new FitWindowsFrameLayout.LayoutParams(AViewGroup.LayoutParams.MatchParent, AViewGroup.LayoutParams.MatchParent);

				return FakeActivityRootView;
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
