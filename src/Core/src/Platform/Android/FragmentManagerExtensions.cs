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
	}
}