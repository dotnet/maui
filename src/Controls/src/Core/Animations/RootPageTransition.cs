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

		/// <summary>Pictures of outgoing roots currently shown over a window, for tests.</summary>
		internal static int ActivePictures;

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
			// The page lays out in the window's direction once it is the window's page (when it doesn't set its own), and
			// it isn't yet, so the direction is worked out from the window.
			var flowDirection = page.FlowDirection == FlowDirection.MatchParent
				? PageTransitionResolver.FlowDirectionOf(window)
				: page.FlowDirection;
			var request = PageTransitionResolver.ResolveRequest(NavigationTransitionKind.Push, from, page, animated, forwardFlowDirection: flowDirection);
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

		/// <summary>
		/// Completes once <paramref name="window"/>'s page has settled after assigning <paramref name="page"/>: at once if it
		/// already is the page, or at the next page change. An assignment made from inside another assignment's change
		/// callbacks is queued until that one finishes, so the native root to animate only exists once this has completed.
		/// The caller checks the page is still <paramref name="page"/>.
		/// </summary>
		internal static Task WhenAssignedAsync(Window window, Page page)
		{
			if (ReferenceEquals(window.Page, page))
			{
				return Task.CompletedTask;
			}

			var assigned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			System.ComponentModel.PropertyChangedEventHandler? handler = null;
			handler = (_, e) =>
			{
				if (e.PropertyName == Window.PageProperty.PropertyName)
				{
					window.PropertyChanged -= handler;
					assigned.TrySetResult(true);
				}
			};
			window.PropertyChanged += handler;
			return assigned.Task;
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

		/// <summary>
		/// Records the root replacement now starting on <paramref name="window"/>, before anything (the page assignment
		/// included) can start another; <paramref name="finish"/> must be idempotent. A replacement started meanwhile
		/// finishes this one, which <see cref="RunningTransition.IsSuperseded"/> then reports.
		/// </summary>
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

			public bool IsSuperseded { get; private set; }

			public void Finish()
			{
				IsSuperseded = true;
				_finish();
			}
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
