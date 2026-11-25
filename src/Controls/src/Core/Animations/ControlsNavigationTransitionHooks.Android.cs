#nullable enable
using Android.Content;
using Android.Provider;
using AAnimation = Android.Views.Animations.Animation;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class ControlsNavigationTransitionHooks
	{
		public object? PrepareNavigation(IView? navigationView, IView? from, IView? to, bool isPopping, bool animated)
		{
			// Resolves which transition runs (and records it for going back) now; providers run only when the base
			// OnCreateNavigationAnimation asks, so an override that doesn't call it keeps them out.
			var kind = isPopping ? NavigationTransitionKind.Pop : NavigationTransitionKind.Push;
			return PageTransitionResolver.Prepare(kind, from as Page, to as Page, animated);
		}

		public bool TryCreateAnimation(object prepared, Context context, bool enter, out AAnimation? animation)
		{
			animation = null;
			if (prepared is not PreparedNavigationTransition navigation)
			{
				return false;
			}

			var platform = navigation.Platform;
			if (platform.IsPlatform)
			{
				return false;
			}

			if (!platform.IsSuppress)
			{
				animation = platform.CreateAnimation(enter);
			}

			return true;
		}
	}

	internal static partial class PageTransitionMotion
	{
		private static partial bool IsReducedMotionEnabledPlatform()
		{
			// "Remove animations" in the accessibility settings sets the animator duration scale to 0.
			var resolver = global::Android.App.Application.Context?.ContentResolver;
			if (resolver is null)
			{
				return false;
			}

			return Settings.Global.GetFloat(resolver, Settings.Global.AnimatorDurationScale, 1f) == 0f;
		}
	}
}
