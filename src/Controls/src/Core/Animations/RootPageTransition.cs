#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Replacing a window's root page with a transition: a picture of the outgoing root is posed out while the
	/// new root is posed in, with the built-in transition's motion.
	/// </summary>
	internal static partial class RootPageTransition
	{
		internal static Task ReplaceAsync(Window window, Page page, PageTransition transition)
		{
			var from = window.Page;
			if (ReferenceEquals(from, page))
			{
				return Task.CompletedTask;
			}

			if (!transition.IsNone)
			{
				PageTransitionResolver.SetPendingForward(page, transition);
			}

			var animated = !transition.IsNone && window.Handler is not null && from is not null;
			var resolved = PageTransitionResolver.Resolve(NavigationTransitionKind.ReplaceRoot, from, page, animated);

			// There is no platform animation for replacing a root page, so Platform swaps without one too.
			// A provider can suppress the transition; MAUI's built-in motion runs otherwise.
			var effective = resolved.Transition;
			if (resolved.IsSuppressed || resolved.IsPlatformDefault || effective.IsPlatform || effective.Duration <= TimeSpan.Zero)
			{
				window.Page = page;
				return Task.CompletedTask;
			}

			return ReplacePlatformAsync(window, page, resolved.Request, effective);
		}

#if !(IOS || MACCATALYST || ANDROID)
		static Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			window.Page = page;
			return Task.CompletedTask;
		}
#endif
	}
}
