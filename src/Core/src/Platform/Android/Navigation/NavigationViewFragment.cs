using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class NavigationViewFragment : Fragment
	{
		AView? _currentView;
		NavigationLayout? _navigationLayout;
		FragmentContainerView? _fragmentContainerView;

		// TODO MAUI: This currently feels like a very unreliable way to retrieve the NavigationLayout
		// If this is called before the Fragment View is parented then this call fails.
		// This should be converted to use Android ViewModels instead of just walking up the visual tree
		NavigationLayout NavigationLayout
		{
			get
			{
				if (_navigationLayout != null)
					return _navigationLayout;

				var view = FragmentContainerView.Parent;

				while (view is not Maui.NavigationLayout && view != null)
					view = view?.Parent;

				_navigationLayout = view as NavigationLayout;

				return _navigationLayout ?? throw new InvalidOperationException($"NavigationLayout cannot be null here");
			}
		}

		FragmentContainerView FragmentContainerView =>
			_fragmentContainerView ??= NavigationLayout.FindViewById<FragmentContainerView>(Resource.Id.nav_host)
			?? throw new InvalidOperationException($"FragmentContainerView cannot be null here");

		// TODO Research ViewModels
		NavigationManager NavigationManager => NavigationLayout.NavigationManager
			?? throw new InvalidOperationException($"Graph cannot be null here");

		protected NavigationViewFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public NavigationViewFragment()
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_fragmentContainerView ??= (FragmentContainerView)container;

			// When shuffling around the back stack sometimes we'll need a page to detach and then reattach.
			// This mainly happens when users are removing or inserting pages. If we only have one page
			// and the user inserts a page at index zero we push a page onto the native backstack so
			// that we can get a toolbar with a back button. 

			// Internally Android destroys the first fragment and then creates a second fragment. 
			// Android removes the view associated with the first fragment and then adds the view 
			// now associated with the second fragment. In our case this is the same view.

			// This is all a bit silly because the page is just getting added and removed to the same
			// view. Unfortunately FragmentContainerView is sealed so we can't inherit from it and influence
			// when the views are being added and removed. If this ends up causing too much shake up
			// Then we can try some other approachs like just modifying the navbar ourselves to include a back button
			// Even if there's only one page on the stack

			var scopedContext = new ScopedMauiContext(
				NavigationManager.MauiContext,
				layoutInflater: inflater,
				fragmentManager: ChildFragmentManager);

			_currentView = NavigationManager.CurrentPage.ToNative(scopedContext);
			_currentView.RemoveFromParent();

			return _currentView;
		}

		public override void OnResume()
		{
			if (_currentView == null || NavigationManager.NavHost == null)
				return;

			if (_currentView.Parent == null)
			{
				// Re-add the view to the container if Android removed it
				// see comment inside OnCreateView for more information
				FragmentContainerView.AddView(_currentView);
			}

			base.OnResume();

		}

		public override void OnDestroyView()
		{
			_navigationLayout = null;
			base.OnDestroyView();
		}

		public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
		{
			int id = 0;

			Animation? returnValue;

			// This means the operation currently being processed shouldn't be animated
			// This will happen if a user inserts or removes a root page
			if (NavigationManager.IsPopping == null || !NavigationManager.IsAnimated)
			{
				returnValue = null;
			}
			else
			{
				// Once we have Function Mappers figured out all of this code can
				// move to a function mapper as a way to customize animations from code
				if (NavigationManager.IsPopping.Value)
				{
					if (!enter)
					{
						id = Resource.Animation.nav_default_pop_exit_anim;
					}
					else
					{
						id = Resource.Animation.nav_default_pop_enter_anim;
					}
				}
				else
				{
					if (enter)
					{
						id = Resource.Animation.nav_default_enter_anim;
					}
					else
					{
						id = Resource.Animation.nav_default_exit_anim;
					}
				}

				if (id > 0)
				{
					returnValue = AnimationUtils.LoadAnimation(Context, id);
				}
				else
				{
					returnValue = base.OnCreateAnimation(transit, enter, id);
				}
			}

			return returnValue!;
		}
	}
}
