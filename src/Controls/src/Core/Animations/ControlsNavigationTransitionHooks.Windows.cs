#nullable enable
using Microsoft.UI.Xaml.Media.Animation;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class ControlsNavigationTransitionHooks
	{
		static readonly SuppressNavigationTransitionInfo s_suppress = new();

		public bool TryGetNavigationTransition(IView? navigationView, IView? from, IView? to, bool isPopping, bool animated, out NavigationTransitionInfo? transition)
		{
			var kind = isPopping ? NavigationTransitionKind.Pop : NavigationTransitionKind.Push;
			var resolved = PageTransitionResolver.Resolve(kind, from as Page, to as Page, animated);

			transition = null;
			if (resolved.IsPlatformDefault)
			{
				return false;
			}

			transition = resolved.IsSuppressed ? s_suppress : resolved.Platform.TransitionInfo;
			return true;
		}
	}

	/// <summary>
	/// MAUI's built-in page transitions on Windows, mapped to the closest WinUI navigation transition.
	/// WinUI's transitions have a fixed duration and curve, so <see cref="PageTransition.Duration"/> and
	/// <see cref="PageTransition.Easing"/> don't apply here, and they raise no completion, so the navigation completes when
	/// WinUI reports it (see the remarks on <see cref="PageTransitions"/>).
	/// </summary>
	internal static class PageTransitionInfos
	{
		internal static PlatformNavigationTransition Create(NavigationTransitionRequest request, PageTransition transition)
		{
			NavigationTransitionInfo info = transition switch
			{
				SlideTransition slide => Slide(slide.From, request.FlowDirection),
				ScaleTransition => new DrillInNavigationTransitionInfo(),
				_ => new EntranceNavigationTransitionInfo(),
			};

			return new PlatformNavigationTransition(info);
		}

		static NavigationTransitionInfo Slide(SlideEdge edge, FlowDirection flowDirection)
		{
			var (x, y) = PageTransitionCurves.SlideVector(edge, flowDirection);
			if (y < 0)
			{
				// WinUI has no slide from the top.
				return new EntranceNavigationTransitionInfo();
			}

			var effect = y > 0
				? SlideNavigationTransitionEffect.FromBottom
				: x > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

			return new SlideNavigationTransitionInfo { Effect = effect };
		}
	}

	internal static partial class PageTransitionMotion
	{
		static readonly global::Windows.UI.ViewManagement.UISettings s_settings = new();

		private static partial bool IsReducedMotionEnabledPlatform() => !s_settings.AnimationsEnabled;
	}
}
