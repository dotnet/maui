using System;
using Android.App;
using Android.Content;
using Android.OS;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class MasterDetailContainer : Xamarin.Forms.Platform.Android.MasterDetailContainer, IManageFragments
	{
		PageContainer _pageContainer;
		FragmentManager _fragmentManager;
		readonly bool _isMaster;
		MasterDetailPage _parent;
		Fragment _currentFragment;
		bool _disposed;

		public MasterDetailContainer(MasterDetailPage parent, bool isMaster, Context context) : base(parent, isMaster, context)
		{
			Id = Platform.GenerateViewId();
			_parent = parent;
			_isMaster = isMaster;
		}

		public bool MarkedForDispose { get; internal set; } = false;

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = ((FormsAppCompatActivity)Context).SupportFragmentManager);

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			// If we're using a PageContainer (i.e., we've wrapped our contents in a Fragment),
			// Make sure that it gets laid out
			if (_pageContainer != null)
			{
				if (_isMaster)
				{
					var controller = (IMasterDetailPageController)_parent;
					var width = (int)Context.ToPixels(controller.MasterBounds.Width);
					// When the base class computes the size of the Master container, it starts at the top of the 
					// screen and adds padding (_parent.MasterBounds.Top) to leave room for the status bar
					// When this container is laid out, it's already starting from the adjusted y value of the parent,
					// so we subtract _parent.MasterBounds.Top from our starting point (to get 0) and add it to the 
					// bottom (so the master page stretches to the bottom of the screen)
					var height = (int)Context.ToPixels(controller.MasterBounds.Height + controller.MasterBounds.Top);
					_pageContainer.Layout(0, 0, width, height);
				}
				else
				{
					_pageContainer.Layout(l, t, r, b);
				}

				_pageContainer.Child.UpdateLayout();
			}
		}

		protected override void AddChildView(VisualElement childView)
		{
			_pageContainer = null;

			Page page = childView as NavigationPage ?? (Page)(childView as TabbedPage);

			if (page == null)
			{
				// The thing we're adding is not a NavigationPage or TabbedPage, so we can just use the old AddChildView 

				if (_currentFragment != null)
				{
					if (!_parent.IsAttachedToRoot())
						return;

					// But first, if the previous occupant of this container was a fragment, we need to remove it properly
					FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
					transaction.RemoveEx(_currentFragment);
					transaction.SetTransitionEx((int)FragmentTransit.None);

					// This is a removal of a fragment that's not going on the back stack; there's no reason to care
					// whether its state gets successfully saved, since we'll never restore it. Ergo, CommitAllowingStateLoss
					transaction.CommitAllowingStateLossEx();

					_currentFragment = null;
				}

				base.AddChildView(childView);
			}
			else
			{
				if (!_parent.IsAttachedToRoot())
					return;

				// The renderers for NavigationPage and TabbedPage both host fragments, so they need to be wrapped in a 
				// FragmentContainer in order to get isolated fragment management
				Fragment fragment = FragmentContainer.CreateInstance(page);

				var fc = fragment as FragmentContainer;

				fc?.SetOnCreateCallback(pc =>
				{
					_pageContainer = pc;
					SetDefaultBackgroundColor(pc.Child);
				});

				FragmentTransaction transaction = FragmentManager.BeginTransactionEx();

				if (_currentFragment != null)
					transaction.RemoveEx(_currentFragment);

				transaction.AddEx(Id, fragment);
				transaction.SetTransitionEx((int)FragmentTransit.None);

				// We don't currently support fragment restoration 
				// So we don't need to worry about loss of this fragment's state
				transaction.CommitAllowingStateLossEx();

				_currentFragment = fragment;

				new Handler(Looper.MainLooper).PostAtFrontOfQueue(() =>
				{
					if (_pageContainer == null)
					{
						// The view we're hosting in the fragment was never created (possibly we're already 
						// navigating to another page?) so there's nothing to commit
						return;
					}

					FragmentManager.ExecutePendingTransactionsEx();
				});
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (_currentFragment != null && !FragmentManager.IsDestroyed)
				{
					FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
					transaction.RemoveEx(_currentFragment);
					transaction.SetTransitionEx((int)FragmentTransit.None);
					transaction.CommitAllowingStateLossEx();
					FragmentManager.ExecutePendingTransactionsEx();

					_currentFragment = null;
				}

				_parent = null;
				_pageContainer = null;
				_fragmentManager = null;
			}

			base.Dispose(disposing);
		}

		public void SetFragmentManager(FragmentManager fragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = fragmentManager;
		}
	}
}