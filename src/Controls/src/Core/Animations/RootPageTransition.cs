#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Replacing a window's root page with a transition: a picture of the outgoing root is posed out while the
	/// new root is posed in, with the built-in transition's motion.
	/// </summary>
	internal static partial class RootPageTransition
	{
		// The root replacement running on each window. A new one finishes it first, so two never animate the same views.
		static readonly ConditionalWeakTable<Window, RunningTransition> s_running = new();

		internal static Task ReplaceAsync(Window window, Page page, PageTransition transition)
		{
			FinishRunning(window);

			var from = window.Page;
			if (ReferenceEquals(from, page))
			{
				return Task.CompletedTask;
			}

			object? token = transition.IsNone ? null : PageTransitionResolver.SetPending(page, NavigationTransitionKind.Push, transition);
			var animated = !transition.IsNone && window.Handler is not null && from is not null;

			// Root replacement isn't part of the provider contract: there is no platform animation to supply, so only
			// MAUI's own motion runs (and Platform, as nothing to animate, swaps without one).
			var request = PageTransitionResolver.ResolveRequest(NavigationTransitionKind.Push, from, page, animated);
			if (token is not null)
			{
				PageTransitionResolver.ClearPending(page, token);
			}

			var effective = request.Transition;
			if (effective.IsNone || effective.IsPlatform || effective.Duration <= TimeSpan.Zero)
			{
				window.Page = page;
				return Task.CompletedTask;
			}

			return ReplacePlatformAsync(window, page, request, effective);
		}

		/// <summary>Ends the root replacement running on <paramref name="window"/>, jumping it to its end state.</summary>
		internal static void FinishRunning(Window window)
		{
			if (s_running.TryGetValue(window, out var running))
			{
				s_running.Remove(window);
				running.Finish();
			}
		}

		/// <summary>Records the root replacement now running on <paramref name="window"/>; <paramref name="finish"/> must be idempotent.</summary>
		static RunningTransition Track(Window window, Action finish)
		{
			var running = new RunningTransition(finish);
			s_running.Put(window, running);
			return running;
		}

		static void Untrack(Window window, RunningTransition running)
		{
			if (s_running.TryGetValue(window, out var current) && ReferenceEquals(current, running))
			{
				s_running.Remove(window);
			}
		}

		sealed class RunningTransition
		{
			readonly Action _finish;

			public RunningTransition(Action finish) => _finish = finish;

			public void Finish() => _finish();
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
