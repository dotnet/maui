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

		Task RunWindowTest<THandler>(IWindow window, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				AViewGroup rootView = MauiContext.Context.GetActivity().Window.DecorView as AViewGroup;
				var decorBackground = rootView.Background;
				var linearLayoutCompat = new LinearLayoutCompat(MauiContext.Context);

				var fragmentManager = MauiContext.GetFragmentManager();
				var viewFragment = new WindowTestFragment(MauiContext, window);

				try
				{
					linearLayoutCompat.Id = AView.GenerateViewId();
					fragmentManager
						.BeginTransaction()
						.Add(linearLayoutCompat.Id, viewFragment)
						.Commit();

					rootView.AddView(linearLayoutCompat);
					await viewFragment.FinishedLoading;

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

					rootView.RemoveView(linearLayoutCompat);

					fragmentManager
						.BeginTransaction()
						.Remove(viewFragment)
						.Commit();

					await linearLayoutCompat.OnUnloadedAsync();
					if (viewFragment.View != null)
						await viewFragment.View.OnUnloadedAsync();

					// This is mainly to remove changes to the decor view that shell imposes
					if (decorBackground != rootView.Background)
						rootView.Background = decorBackground;

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
			readonly IMauiContext _mauiContext;
			readonly IWindow _window;

			public IMauiContext ScopedMauiContext { get; set; }

			public Task FinishedLoading => _taskCompletionSource.Task;

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
		}
	}
}
