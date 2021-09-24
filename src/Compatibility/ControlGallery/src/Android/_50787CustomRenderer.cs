using Android.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
[assembly: ExportRenderer(typeof(NavigationPage), typeof(_50787CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class _50787CustomRenderer : NavigationPageRenderer
	{
		public _50787CustomRenderer(Context context) : base(context)
		{

		}

		protected override int TransitionDuration { get; set; } = 500;

		protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
		{
			if (isPush)
				transaction.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left);
			else
				transaction.SetCustomAnimations(Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
		}
	}
}