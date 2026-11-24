#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Answers Core's navigation managers (see <see cref="NavigationTransitionHooks"/>) from the
	/// page-transition properties and the provider chain.
	/// </summary>
	internal sealed partial class ControlsNavigationTransitionHooks : INavigationTransitionHooks
	{
		internal static readonly ControlsNavigationTransitionHooks Instance = new();

		ControlsNavigationTransitionHooks()
		{
		}

		/// <summary>Installs the hooks. Called when the app is built.</summary>
		internal static void Install() => NavigationTransitionHooks.Current = Instance;
	}

	/// <summary>
	/// MAUI's own transitions: the provider consulted after the app's providers. Built-in presets run on the
	/// platform's navigation animation machinery; heroes (<see cref="SharedTransition"/>) fly inside them.
	/// </summary>
	internal static class BuiltInNavigationTransitions
	{
		/// <summary>
		/// The transition the built-in presets run for <paramref name="request"/>, or <see langword="null"/> to keep the
		/// platform's own animation, with the shared elements that fly inside it.
		/// </summary>
		internal static PageTransition? GetEffectiveTransition(NavigationTransitionRequest request, out List<(VisualElement From, VisualElement To)> heroes)
		{
			var transition = request.Transition;

			// Nothing morphs under reduced motion; a page that only sets shared elements keeps the platform's own
			// transition, which follows the platform's reduced-motion handling.
			heroes = (request.Kind is NavigationTransitionKind.Push or NavigationTransitionKind.Pop) && !PageTransitionResolver.IsReducedMotion
				? SharedTransition.FindPairs(request.From, request.To)
				: new List<(VisualElement From, VisualElement To)>();

			if (transition.IsNone)
			{
				heroes.Clear();
				return null;
			}

			if (transition.IsPlatform)
			{
				// Shared elements need something to fly over: with nothing else set, the pages crossfade.
				if (heroes.Count == 0)
				{
					return null;
				}

				var crossfade = new FadeTransition().Snapshot();

				// The crossfade, not Platform, is what showed the page: going back reverses it even when the heroes no
				// longer match (one hidden or removed), rather than switching to the platform's animation.
				if (request.Kind == NavigationTransitionKind.Push && request.To is { } shownPage)
				{
					PageTransitionResolver.RecordShown(shownPage, crossfade);
				}

				return crossfade;
			}

			return transition;
		}

		internal static PlatformNavigationTransition? GetTransition(NavigationTransitionRequest request)
		{
			var transition = GetEffectiveTransition(request, out var heroes);
			if (transition is null)
			{
				return PlatformNavigationTransition.Platform;
			}

#if IOS || MACCATALYST
			return new PlatformNavigationTransition(new PageTransitionAnimator(request, transition, heroes));
#elif ANDROID
			var animations = new PageTransitionAnimations(request, transition, heroes);
			return new PlatformNavigationTransition(animations.Create);
#elif WINDOWS
			return PageTransitionInfos.Create(request, transition);
#else
			return PlatformNavigationTransition.Platform;
#endif
		}
	}
}
