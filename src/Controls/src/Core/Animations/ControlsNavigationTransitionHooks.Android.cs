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
			var kind = isPopping ? NavigationTransitionKind.Pop : NavigationTransitionKind.Push;
			var resolved = PageTransitionResolver.Resolve(kind, from as Page, to as Page, animated);
			return resolved.IsPlatformDefault ? null : resolved;
		}

		public bool TryCreateAnimation(object prepared, Context context, bool enter, out AAnimation? animation)
		{
			animation = null;
			if (prepared is not ResolvedNavigationTransition resolved || resolved.IsPlatformDefault)
			{
				return false;
			}

			if (!resolved.IsSuppressed)
			{
				animation = resolved.Platform.CreateAnimation(enter);
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
