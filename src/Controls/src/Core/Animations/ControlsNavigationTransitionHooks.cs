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
		/// platform's own animation, with the shared elements that fly inside it. Shared elements are only looked for when
		/// they can change the result: with <paramref name="presetsCarryHeroes"/> false (a platform whose presets don't
		/// host flights), a preset navigation doesn't walk the pages for them at all.
		/// </summary>
		internal static PageTransition? GetEffectiveTransition(NavigationTransitionRequest request, out List<(VisualElement From, VisualElement To)> heroes, bool presetsCarryHeroes = true)
		{
			var transition = request.Transition;
			heroes = new List<(VisualElement From, VisualElement To)>();
			if (transition.IsNone)
			{
				return null;
			}

			// Nothing morphs under reduced motion; a page that only sets shared elements keeps the platform's own
			// transition, which follows the platform's reduced-motion handling.
			var canFly = (request.Kind is NavigationTransitionKind.Push or NavigationTransitionKind.Pop) && !PageTransitionResolver.IsReducedMotion;

			if (transition.IsPlatform)
			{
				if (canFly)
				{
					heroes = SharedTransition.FindPairs(request.From, request.To);
				}

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

			if (canFly && presetsCarryHeroes)
			{
				heroes = SharedTransition.FindPairs(request.From, request.To);
			}

			return transition;
		}

		internal static PlatformNavigationTransition? GetTransition(NavigationTransitionRequest request)
		{
#if WINDOWS
			// WinUI's navigation transitions host no shared elements: a preset doesn't look for them. (Shared elements on
			// a Platform navigation still turn it into the crossfade, as on the other platforms.)
			var transition = GetEffectiveTransition(request, out var heroes, presetsCarryHeroes: false);
#else
			var transition = GetEffectiveTransition(request, out var heroes);
#endif
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
