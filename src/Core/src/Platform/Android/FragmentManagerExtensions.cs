using System;
using Android.Content;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Platform
{
	// This is a way to centralize all fragment modifications which makes it a lot easier to debug
	internal static class FragmentManagerExtensions
	{
		public static FragmentTransaction RemoveEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
		{
			return fragmentTransaction.Remove(fragment);
		}

		public static FragmentTransaction AddEx(this FragmentTransaction fragmentTransaction, int containerViewId, Fragment fragment)
		{
			return fragmentTransaction.Add(containerViewId, fragment);
		}

		public static FragmentTransaction ReplaceEx(this FragmentTransaction fragmentTransaction, int containerViewId, Fragment fragment)
		{
			return fragmentTransaction.Replace(containerViewId, fragment);
		}

		public static FragmentTransaction HideEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
		{
			return fragmentTransaction.Hide(fragment);
		}

		public static FragmentTransaction ShowEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
		{
			return fragmentTransaction.Show(fragment);
		}

		public static FragmentTransaction SetTransitionEx(this FragmentTransaction fragmentTransaction, int transit)
		{
			return fragmentTransaction.SetTransition(transit);
		}

		public static FragmentTransaction SetReorderingAllowedEx(this FragmentTransaction fragmentTransaction, bool reorderingAllowed)
		{
			return fragmentTransaction.SetReorderingAllowed(reorderingAllowed);
		}

		public static int CommitAllowingStateLossEx(this FragmentTransaction fragmentTransaction)
		{
			return fragmentTransaction.CommitAllowingStateLoss();
		}

		public static bool ExecutePendingTransactionsEx(this FragmentManager fragmentManager)
		{
			return fragmentManager.ExecutePendingTransactions();
		}

		public static FragmentTransaction BeginTransactionEx(this FragmentManager fragmentManager)
		{
			return fragmentManager.BeginTransaction();
		}

		public static bool IsDestroyed(this FragmentManager? obj, Context? context)
		{
			if (obj == null || obj.IsDisposed())
				return true;

			if (context == null)
				return true;

			if (obj.IsDestroyed)
				return true;

			return context.IsDestroyed();
		}

		public static IDisposable? RunOrWaitForResume(this FragmentManager obj, Context context, Action<FragmentManager> onResume)
		{
			if (obj.IsDestroyed(context))
				return null;

			if (obj.IsStateSaved)
			{
				var callback = new CallBacks(context, onResume, obj);
				obj.RegisterFragmentLifecycleCallbacks(callback, false);
				return new ActionDisposable(() =>
				{
					callback?.Disconnect();
					callback = null;
				});
			}

			onResume.Invoke(obj);
			return null;
		}

		class CallBacks : FragmentManager.FragmentLifecycleCallbacks
		{
			FragmentManager? _fragmentManager;
			Action<FragmentManager>? _onResume;
			Context? _context;

			public CallBacks(
				Context context,
				Action<FragmentManager> onResume,
				FragmentManager fragmentManager)
			{
				_fragmentManager = fragmentManager;
				_fragmentManager.RegisterFragmentLifecycleCallbacks(this, false);
				_onResume = onResume;
				_context = context;
			}

			public override void OnFragmentDestroyed(FragmentManager fm, Fragment f)
			{
				base.OnFragmentDestroyed(fm, f);
				Disconnect();
			}

			public override void OnFragmentResumed(FragmentManager fm, Fragment f)
			{
				base.OnFragmentResumed(fm, f);
				var resume = _onResume;
				Disconnect();
				resume?.Invoke(fm);
			}

			public void Disconnect()
			{
				if (_fragmentManager is not null &&
					!_fragmentManager.IsDestroyed(_context))
				{
					_fragmentManager.UnregisterFragmentLifecycleCallbacks(this);
				}

				_fragmentManager = null;
				_onResume = null;
				_context = null;
			}
		}

	}
}