using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;

namespace Microsoft.Maui.Platform
{
	[Register("microsoft.maui.platform.MauiNavHostFragment")]
	class MauiNavHostFragment : NavHostFragment
	{
		public StackNavigationManager? StackNavigationManager { get; set; }

		public MauiNavHostFragment()
		{
		}

		protected MauiNavHostFragment(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.Dispose();
		}
	}
}
