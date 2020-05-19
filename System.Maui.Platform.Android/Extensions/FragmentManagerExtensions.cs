
#if __ANDROID_29__
using AndroidX.Fragment.App;
#else
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
#endif

namespace Xamarin.Forms.Platform.Android
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
	}
}